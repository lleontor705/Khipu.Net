namespace Khipu.Core.Report;

using System.Globalization;
using System.Text;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Generador de reportes HTML para documentos electrónicos - Paridad Greenter HtmlReport
/// Genera representación impresa (formato A4) de comprobantes electrónicos.
/// </summary>
public class HtmlReport
{
    private static readonly CultureInfo PeCulture = new("es-PE");

    /// <summary>
    /// Genera HTML de una factura o boleta para impresión.
    /// </summary>
    public string RenderInvoice(Invoice invoice, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Factura Electrónica", p));
        sb.Append(CompanyHeader(invoice.Company, GetDocTypeName(VoucherType.Factura), $"{invoice.Serie}-{invoice.Correlativo}", p));

        // Info del cliente
        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Cliente", invoice.Client.RznSocial));
        sb.Append(InfoRow("RUC/DNI", invoice.Client.NumDoc));
        sb.Append(InfoRow("Fecha Emisión", invoice.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append($"<tr><td>{Encode("Moneda")}</td><td>{GetCurrencyName(invoice.Moneda)}</td></tr>");
        if (invoice.Client.Address != null)
            sb.Append(InfoRow("Dirección", invoice.Client.Address.Direccion));
        sb.Append("</table>");

        // Detalle
        sb.Append(DetailTableHeader());
        foreach (var (detail, i) in invoice.Details.Select((d, i) => (d, i)))
        {
            sb.Append(DetailRow(i + 1, detail, invoice.Moneda));
        }
        sb.Append("</tbody></table>");

        // Totales
        sb.Append("<table class='totals-table'>");
        if (invoice.MtoOperGravadas > 0) sb.Append(TotalRow("Op. Gravadas", invoice.MtoOperGravadas, invoice.Moneda));
        if (invoice.MtoOperExoneradas > 0) sb.Append(TotalRow("Op. Exoneradas", invoice.MtoOperExoneradas, invoice.Moneda));
        if (invoice.MtoOperInafectas > 0) sb.Append(TotalRow("Op. Inafectas", invoice.MtoOperInafectas, invoice.Moneda));
        if (invoice.MtoOperGratuitas > 0) sb.Append(TotalRow("Op. Gratuitas", invoice.MtoOperGratuitas, invoice.Moneda));
        sb.Append(TotalRow("IGV (18%)", invoice.MtoIGV, invoice.Moneda));
        if (invoice.MtoISC > 0) sb.Append(TotalRow("ISC", invoice.MtoISC, invoice.Moneda));
        if (invoice.MtoOtrosTributos > 0) sb.Append(TotalRow("Otros Tributos", invoice.MtoOtrosTributos, invoice.Moneda));
        sb.Append(TotalRow("TOTAL", invoice.MtoImpVenta, invoice.Moneda, bold: true));
        sb.Append("</table>");

        // Leyendas
        if (invoice.Leyendas?.Count > 0)
        {
            sb.Append("<div class='legends'>");
            foreach (var leyenda in invoice.Leyendas)
                sb.Append($"<p>{Encode(leyenda.Value)}</p>");
            sb.Append("</div>");
        }

        // QR + Hash
        sb.Append(QrSection(invoice.Company.Ruc, "01", invoice.Serie, invoice.Correlativo.ToString(),
            invoice.MtoIGV, invoice.MtoImpVenta, invoice.FechaEmision,
            ((int)invoice.Client.TipoDoc).ToString(), invoice.Client.NumDoc, p));

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de una nota de crédito.
    /// </summary>
    public string RenderCreditNote(CreditNote note, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Nota de Crédito Electrónica", p));
        sb.Append(CompanyHeader(note.Company, "NOTA DE CRÉDITO ELECTRÓNICA", $"{note.Serie}-{note.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Cliente", note.Client.RznSocial));
        sb.Append(InfoRow("RUC/DNI", note.Client.NumDoc));
        sb.Append(InfoRow("Fecha Emisión", note.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Doc. Afectado", note.NumDocAfectado));
        sb.Append(InfoRow("Motivo", note.DesMotivo));
        sb.Append("</table>");

        sb.Append(DetailTableHeader());
        foreach (var (detail, i) in note.Details.Select((d, i) => (d, i)))
            sb.Append(DetailRow(i + 1, detail, note.Moneda));
        sb.Append("</tbody></table>");

        sb.Append("<table class='totals-table'>");
        sb.Append(TotalRow("IGV (18%)", note.MtoIGV, note.Moneda));
        sb.Append(TotalRow("TOTAL", note.MtoImpVenta, note.Moneda, bold: true));
        sb.Append("</table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de una guía de remisión.
    /// </summary>
    public string RenderDespatch(Despatch despatch, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Guía de Remisión Electrónica", p));
        sb.Append(CompanyHeader(despatch.Company, "GUÍA DE REMISIÓN ELECTRÓNICA", $"{despatch.Serie}-{despatch.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Destinatario", despatch.Destinatario.RznSocial));
        sb.Append(InfoRow("RUC/DNI", despatch.Destinatario.NumDoc));
        sb.Append(InfoRow("Fecha Emisión", despatch.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Motivo Traslado", despatch.DesMotivoTraslado));
        if (despatch.PuntoPartida != null)
            sb.Append(InfoRow("Punto Partida", despatch.PuntoPartida.Direccion));
        if (despatch.PuntoLlegada != null)
            sb.Append(InfoRow("Punto Llegada", despatch.PuntoLlegada.Direccion));
        if (despatch.PesoTotal.HasValue)
            sb.Append(InfoRow("Peso Total", $"{despatch.PesoTotal:F3} {despatch.UndPesoTotal ?? "KGM"}"));
        if (despatch.Transportista != null)
            sb.Append(InfoRow("Transportista", $"{despatch.Transportista.RznSocial} ({despatch.Transportista.NumDoc})"));
        if (despatch.Vehiculo != null)
            sb.Append(InfoRow("Vehículo", despatch.Vehiculo.Placa));
        sb.Append("</table>");

        // Detalle de ítems
        sb.Append("<table class='detail-table'><thead><tr>");
        sb.Append("<th>#</th><th>Código</th><th>Descripción</th><th>Unidad</th><th>Cantidad</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var (detail, i) in despatch.Details.Select((d, i) => (d, i)))
        {
            sb.Append($"<tr><td>{i + 1}</td><td>{Encode(detail.Codigo)}</td>");
            sb.Append($"<td>{Encode(detail.Descripcion)}</td><td>{detail.Unidad}</td>");
            sb.Append($"<td class='right'>{detail.Cantidad:F2}</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de una nota de débito.
    /// </summary>
    public string RenderDebitNote(DebitNote note, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Nota de Débito Electrónica", p));
        sb.Append(CompanyHeader(note.Company, "NOTA DE DÉBITO ELECTRÓNICA", $"{note.Serie}-{note.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Cliente", note.Client.RznSocial));
        sb.Append(InfoRow("RUC/DNI", note.Client.NumDoc));
        sb.Append(InfoRow("Fecha", note.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Doc. Afectado", note.NumDocAfectado));
        sb.Append(InfoRow("Motivo", note.DesMotivo));
        sb.Append("</table>");

        sb.Append(DetailTableHeader());
        foreach (var (detail, i) in note.Details.Select((d, i) => (d, i)))
            sb.Append(DetailRow(i + 1, detail, note.Moneda));
        sb.Append("</tbody></table>");

        sb.Append("<table class='totals-table'>");
        sb.Append(TotalRow("IGV (18%)", note.MtoIGV, note.Moneda));
        sb.Append(TotalRow("TOTAL", note.MtoImpVenta, note.Moneda, bold: true));
        sb.Append("</table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de una boleta de venta.
    /// </summary>
    public string RenderReceipt(Receipt receipt, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Boleta de Venta Electrónica", p));
        sb.Append(CompanyHeader(receipt.Company, GetDocTypeName(VoucherType.Boleta), $"{receipt.Serie}-{receipt.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Cliente", receipt.Client.RznSocial));
        sb.Append(InfoRow("DNI", receipt.Client.NumDoc));
        sb.Append(InfoRow("Fecha", receipt.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append("</table>");

        sb.Append(DetailTableHeader());
        foreach (var (detail, i) in receipt.Details.Select((d, i) => (d, i)))
            sb.Append(DetailRow(i + 1, detail, receipt.Moneda));
        sb.Append("</tbody></table>");

        sb.Append("<table class='totals-table'>");
        sb.Append(TotalRow("IGV (18%)", receipt.MtoIGV, receipt.Moneda));
        sb.Append(TotalRow("TOTAL", receipt.MtoImpVenta, receipt.Moneda, bold: true));
        sb.Append("</table>");

        sb.Append(QrSection(receipt.Company.Ruc, "03", receipt.Serie, receipt.Correlativo.ToString(),
            receipt.MtoIGV, receipt.MtoImpVenta, receipt.FechaEmision,
            ((int)receipt.Client.TipoDoc).ToString(), receipt.Client.NumDoc, p));

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de un comprobante de percepción.
    /// </summary>
    public string RenderPerception(Perception perception, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Comprobante de Percepción", p));
        sb.Append(CompanyHeader(perception.Company, "COMPROBANTE DE PERCEPCIÓN", $"{perception.Serie}-{perception.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Proveedor", perception.Proveedor.RznSocial));
        sb.Append(InfoRow("RUC", perception.Proveedor.NumDoc));
        sb.Append(InfoRow("Fecha", perception.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Total Percibido", $"S/ {perception.MtoPercepcion:F2}"));
        sb.Append(InfoRow("Total Cobrado", $"S/ {perception.MtoTotalCobrar:F2}"));
        sb.Append("</table>");

        sb.Append("<table class='detail-table'><thead><tr>");
        sb.Append("<th>#</th><th>Tipo</th><th>Documento</th><th>Fecha</th><th>Importe</th><th>Percepción</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var (det, i) in perception.Details.Select((d, i) => (d, i)))
        {
            sb.Append($"<tr><td>{i + 1}</td><td>{Encode(det.TipoDoc)}</td>");
            sb.Append($"<td>{Encode(det.NumDoc)}</td><td>{det.FechaEmision:dd/MM/yyyy}</td>");
            sb.Append($"<td class='right'>S/ {det.ImpTotal:F2}</td>");
            sb.Append($"<td class='right'>S/ {det.Mto:F2}</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de un comprobante de retención.
    /// </summary>
    public string RenderRetention(Retention retention, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Comprobante de Retención", p));
        sb.Append(CompanyHeader(retention.Company, "COMPROBANTE DE RETENCIÓN", $"{retention.Serie}-{retention.Correlativo}", p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Proveedor", retention.Proveedor.RznSocial));
        sb.Append(InfoRow("RUC", retention.Proveedor.NumDoc));
        sb.Append(InfoRow("Fecha", retention.FechaEmision.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Total Retenido", $"S/ {retention.MtoRetencion:F2}"));
        sb.Append(InfoRow("Total Pagado", $"S/ {retention.MtoTotal:F2}"));
        sb.Append("</table>");

        sb.Append("<table class='detail-table'><thead><tr>");
        sb.Append("<th>#</th><th>Tipo</th><th>Documento</th><th>Fecha</th><th>Importe</th><th>Neto a Pagar</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var (det, i) in retention.Details.Select((d, i) => (d, i)))
        {
            sb.Append($"<tr><td>{i + 1}</td><td>{Encode(det.TipoDoc)}</td>");
            sb.Append($"<td>{Encode(det.NumDoc)}</td><td>{det.FechaEmision:dd/MM/yyyy}</td>");
            sb.Append($"<td class='right'>S/ {det.ImpTotal:F2}</td>");
            sb.Append($"<td class='right'>S/ {det.ImpPagar:F2}</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de un resumen diario.
    /// </summary>
    public string RenderSummary(Summary summary, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Resumen Diario", p));
        sb.Append(CompanyHeader(summary.Company, "RESUMEN DIARIO", summary.Correlativo, p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Fecha Generación", summary.FechaGeneracion.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Fecha Envío", summary.FechaEnvio.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Correlativo", summary.Correlativo));
        sb.Append("</table>");

        sb.Append("<table class='detail-table'><thead><tr>");
        sb.Append("<th>#</th><th>Tipo</th><th>Serie-Nro</th><th>Cliente</th><th>Estado</th><th>Total</th><th>IGV</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var (det, i) in summary.Details.Select((d, i) => (d, i)))
        {
            var estado = det.Estado switch { "1" => "Adicionar", "2" => "Modificar", "3" => "Anular", _ => det.Estado };
            sb.Append($"<tr><td>{i + 1}</td><td>{(int)det.TipoDoc:00}</td>");
            sb.Append($"<td>{Encode(det.SerieNro)}</td><td>{Encode(det.ClienteNroDoc)}</td>");
            sb.Append($"<td>{estado}</td>");
            sb.Append($"<td class='right'>{det.Total:F2}</td>");
            sb.Append($"<td class='right'>{det.MtoIGV:F2}</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    /// <summary>
    /// Genera HTML de una comunicación de baja.
    /// </summary>
    public string RenderVoided(Voided voided, ReportParameters? parameters = null)
    {
        var p = parameters ?? new ReportParameters();
        var sb = new StringBuilder();

        sb.Append(HtmlHeader("Comunicación de Baja", p));
        sb.Append(CompanyHeader(voided.Company, "COMUNICACIÓN DE BAJA", voided.Correlativo, p));

        sb.Append("<table class='info-table'>");
        sb.Append(InfoRow("Fecha Generación", voided.FechaGeneracion.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Fecha Envío", voided.FechaEnvio.ToString("dd/MM/yyyy")));
        sb.Append(InfoRow("Correlativo", voided.Correlativo));
        sb.Append("</table>");

        sb.Append("<table class='detail-table'><thead><tr>");
        sb.Append("<th>#</th><th>Tipo Doc</th><th>Serie-Nro</th><th>Motivo de Baja</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var (det, i) in voided.Details.Select((d, i) => (d, i)))
        {
            sb.Append($"<tr><td>{i + 1}</td><td>{Encode(det.TipoDoc)}</td>");
            sb.Append($"<td>{Encode(det.SerieNro)}</td>");
            sb.Append($"<td>{Encode(det.MotivoBaja)}</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append(HtmlFooter());
        return sb.ToString();
    }

    // ===== Helpers =====

    private static string HtmlHeader(string title, ReportParameters p)
    {
        return $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>{title}</title>
<style>
body {{ font-family: Arial, sans-serif; font-size: 11px; margin: 20px; color: #333; }}
.header {{ display: flex; justify-content: space-between; margin-bottom: 15px; border-bottom: 2px solid #333; padding-bottom: 10px; }}
.company-info {{ flex: 1; }}
.company-info h2 {{ margin: 0 0 5px 0; font-size: 14px; }}
.company-info p {{ margin: 2px 0; }}
.doc-box {{ border: 2px solid #333; padding: 10px 20px; text-align: center; min-width: 200px; }}
.doc-box h3 {{ margin: 0 0 5px 0; font-size: 12px; color: #d32f2f; }}
.doc-box .doc-number {{ font-size: 14px; font-weight: bold; }}
.info-table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
.info-table td {{ padding: 3px 8px; border-bottom: 1px solid #eee; }}
.info-table td:first-child {{ font-weight: bold; width: 130px; background: #f5f5f5; }}
.detail-table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
.detail-table th {{ background: #333; color: white; padding: 6px 8px; text-align: left; font-size: 10px; }}
.detail-table td {{ padding: 4px 8px; border-bottom: 1px solid #ddd; }}
.detail-table .right {{ text-align: right; }}
.totals-table {{ width: 300px; margin-left: auto; border-collapse: collapse; margin-top: 10px; }}
.totals-table td {{ padding: 3px 8px; border-bottom: 1px solid #ddd; }}
.totals-table td:first-child {{ text-align: right; }}
.totals-table td:last-child {{ text-align: right; font-family: monospace; }}
.totals-table .total-bold td {{ font-weight: bold; font-size: 13px; border-top: 2px solid #333; }}
.legends {{ margin-top: 10px; padding: 8px; background: #f9f9f9; border: 1px solid #ddd; font-size: 10px; }}
.qr-section {{ margin-top: 15px; text-align: center; font-size: 10px; color: #666; }}
.footer {{ margin-top: 20px; text-align: center; font-size: 9px; color: #999; border-top: 1px solid #ddd; padding-top: 5px; }}
@media print {{ body {{ margin: 0; }} }}
</style></head><body>";
    }

    private static string HtmlFooter()
    {
        return @"<div class='footer'>Representación impresa generada por Khipu.Net</div></body></html>";
    }

    private static string CompanyHeader(Company company, string docType, string docNumber, ReportParameters p)
    {
        var sb = new StringBuilder();
        sb.Append("<div class='header'>");

        // Logo (si existe)
        sb.Append("<div class='company-info'>");
        if (p.LogoBase64 != null)
            sb.Append($"<img src='data:image/png;base64,{p.LogoBase64}' style='max-height:60px;margin-bottom:5px;' /><br/>");
        sb.Append($"<h2>{Encode(company.RazonSocial)}</h2>");
        if (!string.IsNullOrEmpty(company.NombreComercial))
            sb.Append($"<p>{Encode(company.NombreComercial)}</p>");
        sb.Append($"<p>RUC: {company.Ruc}</p>");
        sb.Append($"<p>{Encode(company.Address.Direccion)}</p>");
        sb.Append("</div>");

        // Recuadro del documento
        sb.Append("<div class='doc-box'>");
        sb.Append($"<p>RUC: {company.Ruc}</p>");
        sb.Append($"<h3>{docType}</h3>");
        sb.Append($"<p class='doc-number'>{docNumber}</p>");
        sb.Append("</div>");

        sb.Append("</div>");
        return sb.ToString();
    }

    private static string InfoRow(string label, string? value)
    {
        return $"<tr><td>{Encode(label)}</td><td>{Encode(value ?? "")}</td></tr>";
    }

    private static string DetailTableHeader()
    {
        return @"<table class='detail-table'><thead><tr>
<th>#</th><th>Código</th><th>Descripción</th><th>Und</th><th>Cant.</th>
<th>V. Unit.</th><th>P. Unit.</th><th>Subtotal</th>
</tr></thead><tbody>";
    }

    private static string DetailRow(int num, SaleDetail detail, Currency currency)
    {
        var symbol = GetCurrencySymbol(currency);
        return $@"<tr>
<td>{num}</td>
<td>{Encode(detail.Codigo)}</td>
<td>{Encode(detail.Descripcion)}</td>
<td>{detail.Unidad}</td>
<td class='right'>{detail.Cantidad:F2}</td>
<td class='right'>{symbol} {detail.MtoValorUnitario:F2}</td>
<td class='right'>{symbol} {detail.PrecioVenta:F2}</td>
<td class='right'>{symbol} {detail.MtoValorVenta:F2}</td>
</tr>";
    }

    private static string TotalRow(string label, decimal amount, Currency currency, bool bold = false)
    {
        var symbol = GetCurrencySymbol(currency);
        var cssClass = bold ? " class='total-bold'" : "";
        return $"<tr{cssClass}><td>{label}</td><td>{symbol} {amount:F2}</td></tr>";
    }

    private static string QrSection(string ruc, string tipoDoc, string serie, string correlativo,
        decimal igv, decimal total, DateTime fecha, string clienteTipoDoc, string clienteNumDoc,
        ReportParameters p)
    {
        // Greenter QR format: RUC|TipoDoc|Serie|Correlativo|IGV|Total|Fecha|ClienteTipo|ClienteDoc|
        var qrContent = $"{ruc}|{tipoDoc}|{serie}|{correlativo}|{igv:F2}|{total:F2}|{fecha:yyyy-MM-dd}|{clienteTipoDoc}|{clienteNumDoc}|";

        var sb = new StringBuilder();
        sb.Append("<div class='qr-section'>");
        if (!string.IsNullOrEmpty(p.Hash))
            sb.Append($"<p>Hash: {Encode(p.Hash)}</p>");
        sb.Append($"<p style='font-size:8px;word-break:break-all;'>QR: {Encode(qrContent)}</p>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);

    private static string GetCurrencySymbol(Currency currency) => currency switch
    {
        Currency.Pen => "S/",
        Currency.Usd => "$",
        Currency.Eur => "€",
        _ => "S/"
    };

    private static string GetCurrencyName(Currency currency) => currency switch
    {
        Currency.Pen => "SOLES",
        Currency.Usd => "DÓLARES AMERICANOS",
        Currency.Eur => "EUROS",
        _ => "SOLES"
    };

    private static string GetDocTypeName(VoucherType type) => type switch
    {
        VoucherType.Factura => "FACTURA ELECTRÓNICA",
        VoucherType.Boleta => "BOLETA DE VENTA ELECTRÓNICA",
        VoucherType.NotaCredito => "NOTA DE CRÉDITO ELECTRÓNICA",
        VoucherType.NotaDebito => "NOTA DE DÉBITO ELECTRÓNICA",
        VoucherType.GuiaRemision => "GUÍA DE REMISIÓN ELECTRÓNICA",
        VoucherType.Retencion => "COMPROBANTE DE RETENCIÓN",
        VoucherType.Percepcion => "COMPROBANTE DE PERCEPCIÓN",
        _ => "DOCUMENTO ELECTRÓNICO"
    };
}

/// <summary>
/// Parámetros opcionales para la generación de reportes HTML.
/// </summary>
public class ReportParameters
{
    /// <summary>Logo de la empresa en base64 (PNG/JPEG)</summary>
    public string? LogoBase64 { get; set; }
    /// <summary>Hash/firma digital del documento</summary>
    public string? Hash { get; set; }
    /// <summary>Encabezado personalizado HTML</summary>
    public string? CustomHeader { get; set; }
}
