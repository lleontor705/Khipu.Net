namespace Khipu.Tests;

using Khipu.Core.Factory;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

public class InvoiceFactoryTests
{
    private readonly InvoiceFactory _factory;

    public InvoiceFactoryTests()
    {
        var company = new Company
        {
            Ruc = "20123456789",
            RazonSocial = "EMPRESA SAC"
        };
        _factory = new InvoiceFactory(company);
    }

    [Fact]
    public void CreateInvoice_WithValidData_ReturnsInvoice()
    {
        var client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE SRL" };

        var invoice = _factory.CreateInvoice(client, "F001", 1, DateTime.Now);

        Assert.NotNull(invoice);
        Assert.Equal("F001", invoice.Serie);
        Assert.Equal(1, invoice.Correlativo);
        Assert.Equal(VoucherType.Factura, invoice.TipoDoc);
        Assert.Equal("0101", invoice.TipoOperacion);
    }

    [Fact]
    public void CreateDetail_WithValidData_ReturnsDetailWithCalculations()
    {
        var detail = _factory.CreateDetail("PROD001", "Producto de prueba", "NIU", 2, 100, TaxType.Gravado);

        Assert.Equal("PROD001", detail.Codigo);
        Assert.Equal(2, detail.Cantidad);
        Assert.Equal(100, detail.MtoValorUnitario);
        Assert.Equal(200, detail.MtoValorVenta);
        Assert.Equal(236, detail.PrecioVenta);
        Assert.Equal(TaxType.Gravado, detail.TipoAfectacionIgv);
    }

    [Fact]
    public void CreateDetail_Exonerado_ReturnsDetailWithoutIgv()
    {
        var detail = _factory.CreateDetail("PROD001", "Producto exonerado", "NIU", 1, 100, TaxType.Exonerado);

        Assert.Equal(100, detail.MtoValorVenta);
        Assert.Equal(100, detail.PrecioVenta);
        Assert.Equal(TaxType.Exonerado, detail.TipoAfectacionIgv);
    }

    [Fact]
    public void CreateDetail_WithMidpointValue_RoundsAwayFromZeroForValorVenta()
    {
        var detail = _factory.CreateDetail("PROD001", "Producto midpoint", "NIU", 3, 1.675m, TaxType.Exonerado);

        Assert.Equal(5.03m, detail.MtoValorVenta);
        Assert.Equal(5.03m, detail.PrecioVenta);
    }

    [Fact]
    public void CreateCreditNote_WithValidData_ReturnsCreditNote()
    {
        var client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE SRL" };

        var note = _factory.CreateCreditNote(client, "FC01", 1, DateTime.Now, "F001-100", "01", "Anulación");

        Assert.NotNull(note);
        Assert.Equal("FC01", note.Serie);
        Assert.Equal(VoucherType.NotaCredito, note.TipoDoc);
        Assert.Equal("F001-100", note.NumDocAfectado);
        Assert.Equal("01", note.CodMotivo);
    }
}
