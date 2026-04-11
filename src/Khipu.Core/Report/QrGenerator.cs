namespace Khipu.Core.Report;

using QRCoder;
using Khipu.Data.Documents;

/// <summary>
/// Generador de QR code para comprobantes electrónicos - Paridad Greenter QrRender.
/// Usa QRCoder para generar QR codes reales en formato SVG y PNG base64.
/// </summary>
public static class QrGenerator
{
    /// <summary>
    /// Genera el string de contenido QR para una factura según formato SUNAT.
    /// Formato: RUC|TipoDoc|Serie|Correlativo|IGV|Total|FechaEmision|TipoDocCliente|NumDocCliente|Hash
    /// </summary>
    public static string GetInvoiceQrContent(Invoice invoice, string? hash = null)
    {
        return $"{invoice.Company.Ruc}|01|{invoice.Serie}|{invoice.Correlativo}|{invoice.MtoIGV:F2}|{invoice.MtoImpVenta:F2}|{invoice.FechaEmision:yyyy-MM-dd}|{(int)invoice.Client.TipoDoc}|{invoice.Client.NumDoc}|{hash ?? ""}";
    }

    /// <summary>
    /// Genera el string de contenido QR para una boleta según formato SUNAT.
    /// </summary>
    public static string GetReceiptQrContent(Receipt receipt, string? hash = null)
    {
        return $"{receipt.Company.Ruc}|03|{receipt.Serie}|{receipt.Correlativo}|{receipt.MtoIGV:F2}|{receipt.MtoImpVenta:F2}|{receipt.FechaEmision:yyyy-MM-dd}|{(int)receipt.Client.TipoDoc}|{receipt.Client.NumDoc}|{hash ?? ""}";
    }

    /// <summary>
    /// Genera el string de contenido QR para una guía de remisión.
    /// </summary>
    public static string GetDespatchQrContent(Despatch despatch)
    {
        return $"{despatch.Company.Ruc}|09|{despatch.Serie}|{despatch.Correlativo}|{despatch.FechaEmision:yyyy-MM-dd}|{(int)despatch.Destinatario.TipoDoc}|{despatch.Destinatario.NumDoc}|";
    }

    /// <summary>
    /// Genera un QR code en formato SVG.
    /// Paridad con Greenter QrRender::getImage() que usa BaconQrCode SVG.
    /// </summary>
    public static string GenerateQrSvg(string content, int pixelsPerModule = 4)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var svgQrCode = new SvgQRCode(qrCodeData);
        return svgQrCode.GetGraphic(pixelsPerModule);
    }

    /// <summary>
    /// Genera un QR code como imagen PNG en formato base64 data URI.
    /// Útil para embeber directamente en HTML.
    /// </summary>
    public static string GenerateQrBase64(string content, int pixelsPerModule = 4)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        var pngBytes = pngQrCode.GetGraphic(pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
    }

    /// <summary>
    /// Genera un QR code como bytes PNG.
    /// </summary>
    public static byte[] GenerateQrPng(string content, int pixelsPerModule = 4)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(pixelsPerModule);
    }

    /// <summary>
    /// Genera QR SVG para una factura (contenido SUNAT + rendering).
    /// </summary>
    public static string GenerateInvoiceQrSvg(Invoice invoice, string? hash = null)
    {
        return GenerateQrSvg(GetInvoiceQrContent(invoice, hash));
    }

    /// <summary>
    /// Genera QR base64 PNG para una factura.
    /// </summary>
    public static string GenerateInvoiceQrBase64(Invoice invoice, string? hash = null)
    {
        return GenerateQrBase64(GetInvoiceQrContent(invoice, hash));
    }

    /// <summary>
    /// Genera QR SVG para una guía de remisión.
    /// </summary>
    public static string GenerateDespatchQrSvg(Despatch despatch)
    {
        return GenerateQrSvg(GetDespatchQrContent(despatch));
    }
}
