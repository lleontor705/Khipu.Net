namespace Khipu.Tests;

using System.Net;
using System.Text.Json;
using Khipu.Ws.Models;
using Khipu.Ws.Services;
using Xunit;

public class GreClientTests
{
    [Fact]
    public void Constructor_WithValidCredentials_CreatesInstance()
    {
        using var client = new GreClient("clientId", "secret", "20123456789", "MODDATOS", "password");
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithNullClientId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GreClient(null!, "secret", "20123456789", "MODDATOS", "password"));
    }

    [Fact]
    public void Constructor_WithNullSecret_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GreClient("clientId", null!, "20123456789", "MODDATOS", "password"));
    }

    [Fact]
    public async Task SendBillAsync_WithNoAuth_ReturnsError()
    {
        // Use a handler that fails auth
        var handler = new MockHttpHandler(HttpStatusCode.Unauthorized,
            JsonSerializer.Serialize(new { message = "Unauthorized" }));
        var httpClient = new HttpClient(handler);

        using var client = new GreClient("bad-id", "bad-secret", "20123456789", "MODDATOS", "bad-pass",
            "https://localhost:9999/auth", "https://localhost:9999/cpe", httpClient);

        var request = new SunatSendRequest(new byte[] { 1, 2, 3 }, "test-file");
        var result = await client.SendBillAsync(request);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetCdrAsync_ReturnsNotSupported()
    {
        using var client = new GreClient("id", "secret", "20123456789", "USER", "pass");

        var result = await client.GetCdrAsync(new CdrQuery("20123456789", "01", "F001", 1));

        Assert.False(result.Success);
        Assert.Equal("NOT_SUPPORTED", result.ErrorCode);
    }

    [Fact]
    public async Task GetStatusAsync_WithConnectionError_ReturnsError()
    {
        var handler = new MockHttpHandler(HttpStatusCode.InternalServerError, "Server Error");
        var httpClient = new HttpClient(handler);

        using var client = new GreClient("id", "secret", "20123456789", "USER", "pass",
            "https://localhost:9999/auth", "https://localhost:9999/cpe", httpClient);

        var result = await client.GetStatusAsync("TICKET123");

        Assert.False(result.Success);
    }

    /// <summary>
    /// Mock HTTP handler for testing without real network calls.
    /// </summary>
    private class MockHttpHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public MockHttpHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
