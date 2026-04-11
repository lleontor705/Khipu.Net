namespace Khipu.Tests;

using Khipu.Core.Report;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Generators;
using Xunit;

public class HtmlReportExtendedTests
{
    private readonly HtmlReport _report = new();

    // ===== DebitNote =====
    [Fact]
    public void RenderDebitNote_GeneratesValidHtml()
    {
        var html = _report.RenderDebitNote(DocumentGenerator.CreateDebitNote());
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("FD01-1", html);
        Assert.Contains("F001-00000001", html);
    }

    // ===== Receipt =====
    [Fact]
    public void RenderReceipt_GeneratesValidHtml()
    {
        var html = _report.RenderReceipt(DocumentGenerator.CreateReceipt());
        Assert.Contains("B001-1", html);
        Assert.Contains("JUAN PEREZ", html);
        Assert.Contains("QR:", html);
    }

    // ===== Perception =====
    [Fact]
    public void RenderPerception_GeneratesValidHtml()
    {
        var html = _report.RenderPerception(DocumentGenerator.CreatePerception());
        Assert.Contains("P001-1", html);
        Assert.Contains("detail-table", html);
        Assert.Contains("F001-00000001", html);
    }

    // ===== Retention =====
    [Fact]
    public void RenderRetention_GeneratesValidHtml()
    {
        var html = _report.RenderRetention(DocumentGenerator.CreateRetention());
        Assert.Contains("R001-1", html);
        Assert.Contains("detail-table", html);
        Assert.Contains("F001-00000001", html);
    }

    // ===== Summary =====
    [Fact]
    public void RenderSummary_GeneratesValidHtml()
    {
        var html = _report.RenderSummary(DocumentGenerator.CreateSummary());
        Assert.Contains("RESUMEN DIARIO", html);
        Assert.Contains("001", html);
        Assert.Contains("Adicionar", html);
    }

    // ===== Voided =====
    [Fact]
    public void RenderVoided_GeneratesValidHtml()
    {
        var html = _report.RenderVoided(DocumentGenerator.CreateVoided());
        Assert.Contains("BAJA", html);
        Assert.Contains("Error en documento", html);
    }

    // ===== Edge cases =====
    [Fact]
    public void RenderInvoice_WithNullLogo_OmitsImage()
    {
        var html = _report.RenderInvoice(DocumentGenerator.CreateInvoice(), new ReportParameters { LogoBase64 = null });
        Assert.DoesNotContain("data:image/png;base64", html);
    }

    [Fact]
    public void RenderInvoice_WithHash_ContainsHash()
    {
        var html = _report.RenderInvoice(DocumentGenerator.CreateInvoice(), new ReportParameters { Hash = "abc123hash" });
        Assert.Contains("abc123hash", html);
    }

    [Fact]
    public void RenderDespatch_WithNullTransportista_DoesNotThrow()
    {
        var despatch = DocumentGenerator.CreateDespatch();
        despatch.Transportista = null;
        despatch.Vehiculo = null;
        var html = _report.RenderDespatch(despatch);
        Assert.DoesNotContain("Transportista", html);
        Assert.DoesNotContain("ABC-123", html);
    }

    [Fact]
    public void RenderDespatch_WithNullPuntos_DoesNotThrow()
    {
        var despatch = DocumentGenerator.CreateDespatch();
        despatch.PuntoPartida = null!;
        despatch.PuntoLlegada = null!;
        var html = _report.RenderDespatch(despatch);
        Assert.NotNull(html);
        Assert.DoesNotContain("Punto Partida", html);
    }

    [Fact]
    public void RenderInvoice_WithEmptyLeyendas_NoLegendSection()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        invoice.Leyendas = new List<Legend>();
        var html = _report.RenderInvoice(invoice);
        Assert.DoesNotContain("class='legends'", html);
    }

    [Fact]
    public void RenderInvoice_WithEurCurrency_ShowsEuroSign()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        invoice.Moneda = Currency.Eur;
        var html = _report.RenderInvoice(invoice);
        Assert.Contains("€", html);
    }
}
