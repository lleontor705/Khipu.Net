namespace Khipu.Core.Services;

using Khipu.Core.Constants;
using Khipu.Core.Security;
using Khipu.Data.Documents;
using Khipu.Xml.Builder;
using Khipu.Ws.Services;
using Khipu.Ws.Helpers;
using Khipu.Ws.Models;

/// <summary>
/// Servicio completo para enviar comprobantes a SUNAT (basado en Greenter)
/// </summary>
public class SunatService
{
    private readonly SunatSoapClient _client;
    private readonly XmlSigner? _signer;

    public SunatService(string username, string password, string endpoint, XmlSigner? signer = null)
    {
        _client = new SunatSoapClient(username, password, endpoint);
        _signer = signer;
    }

    /// <summary>
    /// Envía una factura a SUNAT
    /// </summary>
    public async Task<SunatResponse> SendInvoiceAsync(Invoice invoice)
    {
        // 1. Generar XML
        var xmlBuilder = new InvoiceXmlBuilder();
        var xml = xmlBuilder.Build(invoice);
        var fileName = xmlBuilder.GetFileName(invoice);

        // 2. Firmar si hay certificado
        if (_signer != null)
        {
            xml = _signer.Sign(xml);
        }

        // 3. Crear ZIP
        var zipContent = ZipHelper.CreateZip(fileName, xml);

        // 4. Enviar a SUNAT
        return await _client.SendBillAsync(zipContent, fileName);
    }

    /// <summary>
    /// Envía una boleta a SUNAT (va en resumen)
    /// </summary>
    public async Task<SunatResponse> SendReceiptAsync(Receipt receipt)
    {
        // Las boletas van en resumen, no individualmente
        throw new NotImplementedException(\"Use SendSummaryAsync para boletas\");
    }

    /// <summary>
    /// Envía una nota de crédito a SUNAT
    /// </summary>
    public async Task<SunatResponse> SendCreditNoteAsync(CreditNote note)
    {
        var xmlBuilder = new CreditNoteXmlBuilder();
        var xml = xmlBuilder.Build(note);
        var fileName = xmlBuilder.GetFileName(note);

        if (_signer != null)
        {
            xml = _signer.Sign(xml);
        }

        var zipContent = ZipHelper.CreateZip(fileName, xml);
        return await _client.SendBillAsync(zipContent, fileName);
    }

    /// <summary>
    /// Envía un resumen de boletas a SUNAT
    /// </summary>
    public async Task<SunatResponse> SendSummaryAsync(Summary summary)
    {
        var xmlBuilder = new SummaryXmlBuilder();
        var xml = xmlBuilder.Build(summary);
        var fileName = xmlBuilder.GetFileName(summary);

        if (_signer != null)
        {
            xml = _signer.Sign(xml);
        }

        var zipContent = ZipHelper.CreateZip(fileName, xml);
        return await _client.SendSummaryAsync(zipContent, fileName);
    }

    /// <summary>
    /// Envía una comunicación de bajas a SUNAT
    /// </summary>
    public async Task<SunatResponse> SendVoidedAsync(Voided voided)
    {
        var xmlBuilder = new VoidedXmlBuilder();
        var xml = xmlBuilder.Build(voided);
        var fileName = xmlBuilder.GetFileName(voided);

        if (_signer != null)
        {
            xml = _signer.Sign(xml);
        }

        var zipContent = ZipHelper.CreateZip(fileName, xml);
        return await _client.SendSummaryAsync(zipContent, fileName);
    }

    /// <summary>
    /// Consulta el estado de un ticket
    /// </summary>
    public async Task<TicketResponse> GetStatusAsync(string ticket)
    {
        return await _client.GetStatusAsync(ticket);
    }

    /// <summary>
    /// Consulta el CDR de un comprobante
    /// </summary>
    public async Task<CdrResponse> GetCdrAsync(string ruc, string tipoComprobante, string serie, int correlativo)
    {
        return await _client.GetCdrAsync(ruc, tipoComprobante, serie, correlativo);
    }
}
