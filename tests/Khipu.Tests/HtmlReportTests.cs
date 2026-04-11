namespace Khipu.Tests;

using Khipu.Core.Report;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class HtmlReportTests
{
    [Fact]
    public void RenderInvoice_WithValidInvoice_GeneratesHtml()
    {
        var report = new HtmlReport();
        var invoice = CreateTestInvoice();

        var html = report.RenderInvoice(invoice);

        Assert.NotNull(html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("FACTURA ELECTRÓNICA", html);
        Assert.Contains("F001-1", html);
    }

    [Fact]
    public void RenderInvoice_ContainsCompanyInfo()
    {
        var report = new HtmlReport();
        var html = report.RenderInvoice(CreateTestInvoice());

        Assert.Contains("EMPRESA SAC", html);
        Assert.Contains("20123456789", html);
        Assert.Contains("AV. PRINCIPAL 123", html);
    }

    [Fact]
    public void RenderInvoice_ContainsClientInfo()
    {
        var report = new HtmlReport();
        var html = report.RenderInvoice(CreateTestInvoice());

        Assert.Contains("CLIENTE SRL", html);
        Assert.Contains("20987654321", html);
    }

    [Fact]
    public void RenderInvoice_ContainsDetails()
    {
        var report = new HtmlReport();
        var html = report.RenderInvoice(CreateTestInvoice());

        Assert.Contains("Producto de prueba", html);
        Assert.Contains("PROD001", html);
    }

    [Fact]
    public void RenderInvoice_ContainsTotals()
    {
        var report = new HtmlReport();
        var html = report.RenderInvoice(CreateTestInvoice());

        Assert.Contains("IGV (18%)", html);
        Assert.Contains("TOTAL", html);
        Assert.Contains("S/", html);
    }

    [Fact]
    public void RenderInvoice_ContainsQrData()
    {
        var report = new HtmlReport();
        var html = report.RenderInvoice(CreateTestInvoice());

        Assert.Contains("QR:", html);
        Assert.Contains("20123456789|01|F001|1|", html);
    }

    [Fact]
    public void RenderInvoice_WithLegends_ContainsLegends()
    {
        var report = new HtmlReport();
        var invoice = CreateTestInvoice();
        invoice.Leyendas = new List<Legend>
        {
            new() { Code = "1000", Value = "CIENTO DIECIOCHO CON 00/100 SOLES" }
        };

        var html = report.RenderInvoice(invoice);

        Assert.Contains("CIENTO DIECIOCHO CON 00/100 SOLES", html);
    }

    [Fact]
    public void RenderInvoice_WithLogo_ContainsImage()
    {
        var report = new HtmlReport();
        var parameters = new ReportParameters { LogoBase64 = "iVBORw0KGgoAAAANSUhEUg==" };

        var html = report.RenderInvoice(CreateTestInvoice(), parameters);

        Assert.Contains("data:image/png;base64,", html);
    }

    [Fact]
    public void RenderCreditNote_GeneratesHtml()
    {
        var report = new HtmlReport();
        var note = CreateTestCreditNote();

        var html = report.RenderCreditNote(note);

        Assert.NotNull(html);
        Assert.Contains("NOTA DE CRÉDITO", html);
        Assert.Contains("FC01-1", html);
        Assert.Contains("F001-100", html);
        Assert.Contains("Anulaci", html); // HTML-encoded: ó → &#243;
    }

    [Fact]
    public void RenderDespatch_GeneratesHtml()
    {
        var report = new HtmlReport();
        var despatch = CreateTestDespatch();

        var html = report.RenderDespatch(despatch);

        Assert.NotNull(html);
        Assert.Contains("GUÍA DE REMISIÓN", html);
        Assert.Contains("T001-1", html);
        Assert.Contains("Venta de mercader", html); // HTML-encoded: í → &#237;
        Assert.Contains("TRANSPORTES SAC", html);
    }

    [Fact]
    public void RenderDespatch_ContainsAddresses()
    {
        var report = new HtmlReport();
        var html = report.RenderDespatch(CreateTestDespatch());

        Assert.Contains("AV. ORIGEN 456", html);
        Assert.Contains("JR. DESTINO 789", html);
    }

    [Fact]
    public void RenderInvoice_WithUsdCurrency_ShowsDollarSign()
    {
        var report = new HtmlReport();
        var invoice = CreateTestInvoice();
        invoice.Moneda = Currency.Usd;

        var html = report.RenderInvoice(invoice);

        Assert.Contains("$", html);
        Assert.Contains("DÓLARES AMERICANOS", html);
    }

    // ===== Test Data Factories =====

    private Invoice CreateTestInvoice()
    {
        return new Invoice
        {
            Company = CreateTestCompany(),
            Client = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL"
            },
            Serie = "F001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            MtoOperGravadas = 100,
            MtoIGV = 18,
            MtoImpVenta = 118,
            Details = new List<SaleDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Producto de prueba",
                    Unidad = "NIU",
                    Cantidad = 1,
                    MtoValorUnitario = 100,
                    MtoValorVenta = 100,
                    PrecioVenta = 118
                }
            }
        };
    }

    private CreditNote CreateTestCreditNote()
    {
        return new CreditNote
        {
            Company = CreateTestCompany(),
            Client = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL"
            },
            Serie = "FC01",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            TipDocAfectado = "01",
            NumDocAfectado = "F001-100",
            CodMotivo = "01",
            DesMotivo = "Anulación de operación",
            MtoOperGravadas = 50,
            MtoIGV = 9,
            MtoImpVenta = 59,
            Details = new List<SaleDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Producto devuelto",
                    Unidad = "NIU",
                    Cantidad = 1,
                    MtoValorUnitario = 50,
                    MtoValorVenta = 50,
                    PrecioVenta = 59
                }
            }
        };
    }

    private Despatch CreateTestDespatch()
    {
        return new Despatch
        {
            Company = CreateTestCompany(),
            Destinatario = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL"
            },
            Serie = "T001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            CodMotivoTraslado = "01",
            DesMotivoTraslado = "Venta de mercadería",
            PesoTotal = 25.5m,
            UndPesoTotal = "KGM",
            PuntoPartida = new Address { Direccion = "AV. ORIGEN 456, LIMA" },
            PuntoLlegada = new Address { Direccion = "JR. DESTINO 789, CALLAO" },
            Transportista = new Transportist
            {
                TipoDoc = "6",
                NumDoc = "20111222333",
                RznSocial = "TRANSPORTES SAC"
            },
            Details = new List<DespatchDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Producto",
                    Unidad = "NIU",
                    Cantidad = 10
                }
            }
        };
    }

    private Company CreateTestCompany()
    {
        return new Company
        {
            Ruc = "20123456789",
            RazonSocial = "EMPRESA SAC",
            Address = new Address
            {
                Ubigeo = "150101",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "LIMA",
                Direccion = "AV. PRINCIPAL 123",
                CodigoLocal = "0000"
            }
        };
    }
}
