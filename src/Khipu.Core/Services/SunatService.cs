namespace Khipu.Core.Services;

using Khipu.Core.Security;
using Khipu.Data.Documents;
using Khipu.Ws.Helpers;
using Khipu.Ws.Interfaces;
using Khipu.Ws.Models;
using Khipu.Ws.Services;
using Khipu.Xml.Builder;
using Khipu.Xml.Interfaces;

public class SunatService
{
    private const string PendingStatusCode = "98";

    private readonly ISunatClient _client;
    private readonly XmlSigner? _signer;

    public SunatService(string username, string password, string endpoint, XmlSigner? signer = null)
        : this(new SunatSoapClient(username, password, endpoint), signer)
    {
    }

    public SunatService(ISunatClient client, XmlSigner? signer = null)
    {
        _client = client;
        _signer = signer;
    }

    public Task<SunatResponse> SendInvoiceAsync(Invoice invoice)
        => SendBillAsync(invoice, new InvoiceXmlBuilder());

    public Task<SunatResponse> SendCreditNoteAsync(CreditNote note)
        => SendBillAsync(note, new CreditNoteXmlBuilder());

    public Task<SunatResponse> SendSummaryAsync(Summary summary)
        => SendSummaryLikeAsync(summary, new SummaryXmlBuilder());

    public Task<SunatResponse> SendVoidedAsync(Voided voided)
        => SendSummaryLikeAsync(voided, new VoidedXmlBuilder());

    public Task<SunatResponse> SendDebitNoteAsync(DebitNote note)
        => SendBillAsync(note, new DebitNoteXmlBuilder());

    public Task<SunatResponse> SendReceiptAsync(Receipt receipt)
        => SendBillAsync(receipt, new ReceiptXmlBuilder());

    public Task<SunatResponse> SendDespatchAsync(Despatch despatch)
        => SendBillAsync(despatch, new DespatchXmlBuilder());

    public Task<TicketResponse> GetStatusAsync(string ticket)
        => _client.GetStatusAsync(ticket);

    public Task<CdrResponse> GetCdrAsync(string ruc, string tipoComprobante, string serie, int correlativo)
        => _client.GetCdrAsync(new CdrQuery(ruc, tipoComprobante, serie, correlativo));

    public async Task<CdrResponse> SendSummaryAndQueryCdrAsync(
        Summary summary,
        string tipoComprobante,
        string serie,
        int correlativo,
        int maxStatusChecks = 3,
        TimeSpan? pollInterval = null)
    {
        Internal.Guard.NotNull(summary);
        Internal.Guard.NotNullOrWhiteSpace(tipoComprobante);
        Internal.Guard.NotNullOrWhiteSpace(serie);

        var ruc = summary.Company?.Ruc;
        if (string.IsNullOrWhiteSpace(ruc))
        {
            return new CdrResponse
            {
                Success = false,
                IsAccepted = false,
                ErrorCode = "INVALID_DOCUMENT",
                ErrorMessage = "RUC de emisor requerido para consultar CDR",
            };
        }

        if (correlativo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(correlativo));
        }

        if (maxStatusChecks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxStatusChecks));
        }

        var effectivePollInterval = pollInterval ?? TimeSpan.FromMilliseconds(250);
        if (effectivePollInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval));
        }

        var send = await SendSummaryAsync(summary).ConfigureAwait(false);
        if (!send.Success)
        {
            return new CdrResponse
            {
                Success = false,
                IsAccepted = false,
                ErrorCode = string.IsNullOrWhiteSpace(send.ErrorCode) ? "SEND_ERROR" : send.ErrorCode,
                ErrorMessage = send.ErrorMessage,
            };
        }

        if (string.IsNullOrWhiteSpace(send.Ticket))
        {
            return await GetCdrAsync(ruc, tipoComprobante, serie, correlativo).ConfigureAwait(false);
        }

        var ticket = send.Ticket;
        var lastStatusCode = send.StatusCode;

        for (var attempt = 0; attempt < maxStatusChecks; attempt++)
        {
            var status = await GetStatusAsync(ticket).ConfigureAwait(false);
            if (!status.Success)
            {
                return new CdrResponse
                {
                    Success = false,
                    IsAccepted = false,
                    ErrorCode = "STATUS_ERROR",
                    ErrorMessage = status.ErrorMessage,
                };
            }

            lastStatusCode = status.StatusCode;
            if (!string.Equals(lastStatusCode, PendingStatusCode, StringComparison.Ordinal))
            {
                return await GetCdrAsync(ruc, tipoComprobante, serie, correlativo).ConfigureAwait(false);
            }

            if (attempt + 1 < maxStatusChecks && effectivePollInterval > TimeSpan.Zero)
            {
                await Task.Delay(effectivePollInterval).ConfigureAwait(false);
            }
        }

        return new CdrResponse
        {
            Success = false,
            IsAccepted = false,
            ErrorCode = PendingStatusCode,
            Notes = $"Ticket {ticket} permanece pendiente ({lastStatusCode ?? PendingStatusCode})",
        };
    }

    private async Task<SunatResponse> SendBillAsync<TDocument>(TDocument document, IXmlBuilder<TDocument> xmlBuilder)
        where TDocument : class
    {
        var payload = BuildZipPayload(document, xmlBuilder);
        var zipContent = payload.zipContent;
        var fileNameWithoutExtension = payload.fileNameWithoutExtension;
        return await _client.SendBillAsync(new SunatSendRequest(zipContent, fileNameWithoutExtension)).ConfigureAwait(false);
    }

    private async Task<SunatResponse> SendSummaryLikeAsync<TDocument>(TDocument document, IXmlBuilder<TDocument> xmlBuilder)
        where TDocument : class
    {
        var payload = BuildZipPayload(document, xmlBuilder);
        var zipContent = payload.zipContent;
        var fileNameWithoutExtension = payload.fileNameWithoutExtension;
        return await _client.SendSummaryAsync(new SunatSendRequest(zipContent, fileNameWithoutExtension)).ConfigureAwait(false);
    }

    private (byte[] zipContent, string fileNameWithoutExtension) BuildZipPayload<TDocument>(TDocument document, IXmlBuilder<TDocument> xmlBuilder)
        where TDocument : class
    {
        var xml = xmlBuilder.Build(document);
        if (_signer is not null)
        {
            xml = _signer.Sign(xml);
        }

        var fileName = xmlBuilder.GetFileName(document);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
        var zipContent = ZipHelper.CreateZip(fileNameWithoutExtension, xml);
        return (zipContent, fileNameWithoutExtension);
    }
}
