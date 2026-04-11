namespace Khipu.Tests;

using System.Net;
using System.Text.Json;
using Khipu.Ws.Models;
using Khipu.Ws.Services;
using Xunit;

public class GreClientExtendedTests
{
    [Fact]
    public async Task SendBillAsync_WithSuccessfulAuth_SendsWithBearer()
    {
        // Simulates: 1st call = token, 2nd call = sendBill
        var callCount = 0;
        var handler = new SequentialHandler(request =>
        {
            callCount++;
            if (callCount == 1) // Token request
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { access_token = "test-token", token_type = "Bearer", expires_in = 3600 }),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }
            // Send bill
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { numTicket = "TICKET123", fecRecepcion = "2026-04-11" }),
                    System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new GreClient("id", "secret", "20100070970", "USER", "pass",
            "https://auth.test", "https://cpe.test", new HttpClient(handler));

        var result = await client.SendBillAsync(new SunatSendRequest(new byte[] { 1, 2 }, "test"));

        Assert.True(result.Success);
        Assert.Equal("TICKET123", result.Ticket);
    }

    [Fact]
    public async Task GetStatusAsync_WithCdrGenerated_ReturnsCdrZip()
    {
        var callCount = 0;
        var handler = new SequentialHandler(request =>
        {
            callCount++;
            if (callCount == 1) // Token
                return TokenResponse();
            // Status with CDR
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { codRespuesta = "0", indCdrGenerado = "1", arcCdr = Convert.ToBase64String(new byte[] { 1, 2, 3 }) }),
                    System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new GreClient("id", "secret", "20100070970", "USER", "pass",
            "https://auth.test", "https://cpe.test", new HttpClient(handler));

        var result = await client.GetStatusAsync("TICKET123");

        Assert.True(result.Success);
        Assert.Equal("0", result.StatusCode);
        Assert.NotNull(result.CdrZip);
    }

    [Fact]
    public async Task GetStatusAsync_WithPending_ReturnsPendingCode()
    {
        var callCount = 0;
        var handler = new SequentialHandler(request =>
        {
            callCount++;
            if (callCount == 1) return TokenResponse();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { codRespuesta = "98", indCdrGenerado = "0" }),
                    System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new GreClient("id", "secret", "20100070970", "USER", "pass",
            "https://auth.test", "https://cpe.test", new HttpClient(handler));

        var result = await client.GetStatusAsync("T1");

        Assert.True(result.Success);
        Assert.Equal("98", result.StatusCode);
        Assert.Null(result.CdrZip);
    }

    [Fact]
    public async Task SendBillAsync_WithAuthFailure_ReturnsError()
    {
        var handler = new SequentialHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"message\":\"Invalid credentials\"}", System.Text.Encoding.UTF8, "application/json")
        });

        using var client = new GreClient("bad", "bad", "20100070970", "USER", "pass",
            "https://auth.test", "https://cpe.test", new HttpClient(handler));

        var result = await client.SendBillAsync(new SunatSendRequest(new byte[] { 1 }, "test"));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task EnsureToken_CachesToken_DoesNotRefetchOnSecondCall()
    {
        var tokenCallCount = 0;
        var handler = new SequentialHandler(request =>
        {
            if (request.RequestUri!.PathAndQuery.Contains("oauth2/token"))
            {
                tokenCallCount++;
                return TokenResponse();
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"numTicket\":\"T1\"}", System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new GreClient("id", "secret", "20100070970", "USER", "pass",
            "https://auth.test", "https://cpe.test", new HttpClient(handler));

        await client.SendBillAsync(new SunatSendRequest(new byte[] { 1 }, "a"));
        await client.SendBillAsync(new SunatSendRequest(new byte[] { 2 }, "b"));

        Assert.Equal(1, tokenCallCount); // Token fetched only once
    }

    private static HttpResponseMessage TokenResponse() => new(HttpStatusCode.OK)
    {
        Content = new StringContent(
            JsonSerializer.Serialize(new { access_token = "tok", token_type = "Bearer", expires_in = 3600 }),
            System.Text.Encoding.UTF8, "application/json")
    };

    private class SequentialHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public SequentialHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_handler(request));
    }
}
