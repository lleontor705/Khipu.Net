namespace Khipu.Tests;

using Khipu.Core.Report;
using Khipu.Data.Generators;
using Xunit;

public class QrGeneratorTests
{
    [Fact]
    public void GetInvoiceQrContent_FollowsSunatFormat()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        var qr = QrGenerator.GetInvoiceQrContent(invoice, "abc123");

        Assert.Contains("20100070970", qr); // RUC
        Assert.Contains("|01|", qr); // Tipo doc
        Assert.Contains("|F001|", qr); // Serie
        Assert.Contains("|abc123", qr); // Hash
        // Format: RUC|TipoDoc|Serie|Corr|IGV|Total|Fecha|TipoCliente|NumCliente|Hash
        Assert.Equal(9, qr.Count(c => c == '|'));
    }

    [Fact]
    public void GetReceiptQrContent_HasBoletaType()
    {
        var receipt = DocumentGenerator.CreateReceipt();
        var qr = QrGenerator.GetReceiptQrContent(receipt);
        Assert.Contains("|03|", qr); // Boleta type
        Assert.Contains("|B001|", qr);
    }

    [Fact]
    public void GetDespatchQrContent_HasGuiaType()
    {
        var despatch = DocumentGenerator.CreateDespatch();
        var qr = QrGenerator.GetDespatchQrContent(despatch);
        Assert.Contains("|09|", qr); // Guia type
        Assert.Contains("|T001|", qr);
    }

    [Fact]
    public void GenerateQrSvg_ReturnsValidSvg()
    {
        var svg = QrGenerator.GenerateQrSvg("test content");
        Assert.Contains("<svg", svg);
        Assert.Contains("</svg>", svg);
    }

    [Fact]
    public void GenerateQrBase64_ReturnsDataUri()
    {
        var base64 = QrGenerator.GenerateQrBase64("test content");
        Assert.StartsWith("data:image/png;base64,", base64);
    }

    [Fact]
    public void GenerateQrPng_ReturnsPngBytes()
    {
        var bytes = QrGenerator.GenerateQrPng("test content");
        Assert.NotEmpty(bytes);
        // PNG magic bytes
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal(0x50, bytes[1]); // P
        Assert.Equal(0x4E, bytes[2]); // N
        Assert.Equal(0x47, bytes[3]); // G
    }

    [Fact]
    public void GenerateInvoiceQrSvg_ProducesValidSvg()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        var svg = QrGenerator.GenerateInvoiceQrSvg(invoice, "testHash");
        Assert.Contains("<svg", svg);
    }

    [Fact]
    public void GenerateDespatchQrSvg_ProducesValidSvg()
    {
        var despatch = DocumentGenerator.CreateDespatch();
        var svg = QrGenerator.GenerateDespatchQrSvg(despatch);
        Assert.Contains("<svg", svg);
    }
}
