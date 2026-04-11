namespace Khipu.Ws.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Khipu.Ws.Interfaces;
using Khipu.Ws.Models;

public class SunatSoapClient : ISunatClient
{
    private static readonly XNamespace SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Ser = "http://service.sunat.gob.pe";

    private readonly HttpClient _httpClient;

    public SunatSoapClient(string username, string password, string endpoint, HttpClient? httpClient = null)
    {
        Internal.Guard.NotNullOrWhiteSpace(username);
        Internal.Guard.NotNullOrWhiteSpace(password);
        Internal.Guard.NotNullOrWhiteSpace(endpoint);

        _httpClient = httpClient ?? new HttpClient();
        _httpClient.BaseAddress = new Uri(endpoint, UriKind.Absolute);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Khipu.Net", "1.0"));
    }

    public Task<SunatResponse> SendBillAsync(SunatSendRequest request, CancellationToken cancellationToken = default)
        => SendAsync("sendBill", request, cancellationToken);

    public Task<SunatResponse> SendSummaryAsync(SunatSendRequest request, CancellationToken cancellationToken = default)
        => SendAsync("sendSummary", request, cancellationToken);

    public async Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = BuildEnvelope("getStatus", new XElement[] { new XElement("ticket", ticket) });
            var xml = await PostSoapAsync(envelope, cancellationToken).ConfigureAwait(false);
            var doc = XDocument.Parse(xml);

            var fault = ParseFault(doc);
            var payload = new SunatTransportPayload(
                StatusCode: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "statusCode")?.Value,
                Content: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "content")?.Value,
                Fault: fault);

            return SunatResponseMapper.MapStatus(payload, ticket);
        }
        catch (Exception ex)
        {
            return SunatResponseMapper.MapStatusTransportException(ex);
        }
    }

    public async Task<CdrResponse> GetCdrAsync(CdrQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = BuildEnvelope(
                "getStatusCdr",
                new XElement[]
                {
                    new XElement("rucComprobante", query.Ruc),
                    new XElement("tipoComprobante", query.TipoComprobante),
                    new XElement("serieComprobante", query.Serie),
                    new XElement("numeroComprobante", query.Correlativo)
                });

            var xml = await PostSoapAsync(envelope, cancellationToken).ConfigureAwait(false);
            var doc = XDocument.Parse(xml);

            var fault = ParseFault(doc);
            var payload = new SunatTransportPayload(
                StatusCode: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "statusCode")?.Value,
                StatusMessage: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "statusMessage")?.Value,
                Content: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "content")?.Value,
                Fault: fault);

            return SunatResponseMapper.MapCdr(payload);
        }
        catch (Exception ex)
        {
            return SunatResponseMapper.MapCdrTransportException(ex);
        }
    }

    private async Task<SunatResponse> SendAsync(string operation, SunatSendRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var envelope = BuildEnvelope(
                operation,
                new XElement[]
                {
                    new XElement("fileName", request.FileNameWithoutExtension),
                    new XElement("contentFile", Convert.ToBase64String(request.ZipContent))
                });

            var xml = await PostSoapAsync(envelope, cancellationToken).ConfigureAwait(false);
            var doc = XDocument.Parse(xml);

            var fault = ParseFault(doc);
            var payload = new SunatTransportPayload(
                Ticket: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ticket")?.Value,
                ApplicationResponse: doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "applicationResponse")?.Value,
                Fault: fault);

            return SunatResponseMapper.MapSend(payload);
        }
        catch (Exception ex)
        {
            return SunatResponseMapper.MapSendTransportException(ex);
        }
    }

    private async Task<string> PostSoapAsync(string envelope, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return responseContent;
    }

    private static string BuildEnvelope(string operation, IEnumerable<XElement> bodyElements)
    {
        var operationElement = new XElement(Ser + operation, bodyElements);
        var envelope = new XDocument(
            new XElement(SoapEnv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", SoapEnv),
                new XAttribute(XNamespace.Xmlns + "ser", Ser),
                new XElement(SoapEnv + "Header"),
                new XElement(SoapEnv + "Body", operationElement)));
        return envelope.ToString(SaveOptions.DisableFormatting);
    }

    private static string? ParseFault(XDocument doc)
    {
        var fault = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Fault");
        if (fault is null)
        {
            return null;
        }

        var faultCode = fault.Descendants().FirstOrDefault(x => x.Name.LocalName == "faultcode")?.Value;
        var faultString = fault.Descendants().FirstOrDefault(x => x.Name.LocalName == "faultstring")?.Value;
        return string.Join(" - ", new[] { faultCode, faultString }.Where(v => !string.IsNullOrWhiteSpace(v)));
    }

}
