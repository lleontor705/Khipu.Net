namespace Khipu.Tests;

using System.Net;
using System.Text;
using System.Text.Json;
using Khipu.Ws.Models;
using Khipu.Ws.Services;

public class SunatSoapClientFixtureMatrixTests
{
    [Fact]
    public async Task SoapCdrMatrix_CoversSuccessPendingWarningFaultCorruptHttpAndTimeout()
    {
        var matrix = JsonSerializer.Deserialize<List<SoapFixtureCase>>(ReadFixture("soap-cdr-matrix.json"), JsonOptions) ?? [];

        foreach (var testCase in matrix)
        {
            var fixture = BuildFixture(testCase.Mode);
            var http = new HttpClient(new FixtureHandler(fixture)) { BaseAddress = new Uri("https://example.test/ws") };
            var client = new SunatSoapClient("user", "pass", "https://example.test/ws", http);

            switch (testCase.Api)
            {
                case "SendBill":
                {
                    var response = await client.SendBillAsync(new SunatSendRequest(CreateZip(), "20123456789-01-F001-00000001"));
                    Assert.Equal(testCase.ExpectedSuccess, response.Success);
                    Assert.Equal(testCase.ExpectedErrorCode, response.ErrorCode);
                    Assert.Equal(testCase.ExpectedStatusCode, response.StatusCode);
                    Assert.Equal(testCase.ExpectedClassification, SunatSoapResponseMapper.Classify(response).Code.ToString());
                    break;
                }
                case "SendSummary":
                {
                    var response = await client.SendSummaryAsync(new SunatSendRequest(CreateZip(), "20123456789-RC-20260326-001"));
                    Assert.Equal(testCase.ExpectedSuccess, response.Success);
                    Assert.Equal(testCase.ExpectedErrorCode, response.ErrorCode);
                    Assert.Equal(testCase.ExpectedStatusCode, response.StatusCode);
                    Assert.Equal(testCase.ExpectedClassification, SunatSoapResponseMapper.Classify(response).Code.ToString());
                    break;
                }
                case "GetStatus":
                {
                    var response = await client.GetStatusAsync("ABC123");
                    Assert.Equal(testCase.ExpectedSuccess, response.Success);
                    Assert.Equal(testCase.ExpectedStatusCode, response.StatusCode);
                    Assert.Equal(testCase.ExpectedClassification, SunatSoapResponseMapper.Classify(response).Code.ToString());
                    break;
                }
                case "GetCdr":
                {
                    var response = await client.GetCdrAsync(new CdrQuery("20123456789", "01", "F001", 1));
                    Assert.Equal(testCase.ExpectedSuccess, response.Success);
                    Assert.Equal(testCase.ExpectedErrorCode, response.ErrorCode);
                    Assert.Equal(testCase.ExpectedClassification, SunatSoapResponseMapper.Classify(response).Code.ToString());
                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported API in fixture: {testCase.Api}");
            }
        }
    }

    [Fact]
    public async Task SoapClient_SetsBasicAuthAndUserAgentHeaders()
    {
        var fixture = BuildFixture("success-cdr");
        HttpRequestMessage? capturedRequest = null;

        var http = new HttpClient(new FixtureHandler(fixture, req => capturedRequest = req))
        {
            BaseAddress = new Uri("https://example.test/ws")
        };

        var client = new SunatSoapClient("user", "pass", "https://example.test/ws", http);
        _ = await client.SendBillAsync(new SunatSendRequest(CreateZip(), "20123456789-01-F001-00000001"));

        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest!.Headers.Authorization);
        Assert.Equal("Basic", capturedRequest.Headers.Authorization!.Scheme);

        var rawAuth = Encoding.ASCII.GetString(Convert.FromBase64String(capturedRequest.Headers.Authorization.Parameter!));
        Assert.Equal("user:pass", rawAuth);

        Assert.Contains(capturedRequest.Headers.UserAgent, ua =>
            string.Equals(ua.Product?.Name, "Khipu.Net", StringComparison.Ordinal) &&
            string.Equals(ua.Product?.Version, "1.0", StringComparison.Ordinal));
    }

    private static Fixture BuildFixture(string mode)
    {
        var acceptedCdr = Convert.ToBase64String(CreateZip("R-20123456789-01-F001-00000001.xml", "<cdr><code>0</code></cdr>"));
        var warningCdr = Convert.ToBase64String(CreateZip("R-20123456789-01-F001-00000001.xml", "<cdr><code>99</code></cdr>"));
        var corruptCdr = Convert.ToBase64String(Encoding.UTF8.GetBytes("not-a-zip"));

        return mode switch
        {
            "success-cdr" => new Fixture(WrapSoap("<sendBillResponse><applicationResponse>" + acceptedCdr + "</applicationResponse></sendBillResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "pending-ticket" => new Fixture(WrapSoap("<sendSummaryResponse><ticket>1234567890</ticket></sendSummaryResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "pending" => new Fixture(WrapSoap("<getStatusResponse><status><statusCode>98</statusCode></status></getStatusResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "accepted" => new Fixture(WrapSoap("<getStatusCdrResponse><statusCdr><statusCode>0</statusCode><statusMessage>Aceptado</statusMessage><content>" + acceptedCdr + "</content></statusCdr></getStatusCdrResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "warning" => new Fixture(WrapSoap("<getStatusCdrResponse><statusCdr><statusCode>99</statusCode><statusMessage>Observado</statusMessage><content>" + warningCdr + "</content></statusCdr></getStatusCdrResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "corrupt-cdr" => new Fixture(WrapSoap("<getStatusCdrResponse><statusCdr><statusCode>0</statusCode><statusMessage>Aceptado</statusMessage><content>" + corruptCdr + "</content></statusCdr></getStatusCdrResponse>"), HttpStatusCode.OK, SimulateTimeout: false),
            "fault" => new Fixture(WrapSoapFault("soapenv:Server", "Error de procesamiento"), HttpStatusCode.OK, SimulateTimeout: false),
            "http-error" => new Fixture("<html><body>server error</body></html>", HttpStatusCode.InternalServerError, SimulateTimeout: false),
            "timeout" => new Fixture(string.Empty, HttpStatusCode.OK, SimulateTimeout: true),
            _ => throw new InvalidOperationException($"Unsupported fixture mode: {mode}")
        };
    }

    private static string WrapSoap(string body) =>
        "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body>" + body + "</soapenv:Body></soapenv:Envelope>";

    private static string WrapSoapFault(string code, string text) =>
        "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><soapenv:Fault><faultcode>" + code + "</faultcode><faultstring>" + text + "</faultstring></soapenv:Fault></soapenv:Body></soapenv:Envelope>";

    private static byte[] CreateZip(string entryName = "doc.xml", string xml = "<root/>")
        => Khipu.Ws.Helpers.ZipHelper.CreateZip(entryName, xml);

    private static string ReadFixture(string relativePath)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record Fixture(string ResponseBody, HttpStatusCode StatusCode, bool SimulateTimeout);

    private sealed class FixtureHandler(Fixture fixture, Action<HttpRequestMessage>? onRequest = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest?.Invoke(request);

            if (fixture.SimulateTimeout)
            {
                throw new TaskCanceledException("Simulated timeout");
            }

            var response = new HttpResponseMessage(fixture.StatusCode)
            {
                Content = new StringContent(fixture.ResponseBody, Encoding.UTF8, "text/xml")
            };

            return Task.FromResult(response);
        }
    }
}

public sealed class SoapFixtureCase
{
    public string Id { get; init; } = string.Empty;
    public string Api { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public bool ExpectedSuccess { get; init; }
    public string? ExpectedErrorCode { get; init; }
    public string? ExpectedStatusCode { get; init; }
    public string ExpectedClassification { get; init; } = string.Empty;
}
