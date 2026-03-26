namespace Khipu.Tests;

using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class InvoiceBuilderExtendedTests
{
    [Fact]
    public void BuildInvoice_WithMultipleDetails_CalculatesCorrectTotals()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "20123456789", RazonSocial = "TEST" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENT" })
            .WithSerie("F001")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now)
            .AddDetail(new SaleDetail { Cantidad = 2, MtoValorUnitario = 100, MtoValorVenta = 200 })
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 500, MtoValorVenta = 500 });

        var invoice = builder.Build();

        Assert.Equal(700, invoice.MtoOperGravadas);
        Assert.Equal(126, invoice.MtoIGV); // 18% de 700
        Assert.Equal(826, invoice.MtoImpVenta);
    }

    [Fact]
    public void Validate_WithInvalidRuc_ReturnsFalseAndError()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "INVALID", RazonSocial = "TEST" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENT" })
            .WithSerie("F001")
            .WithCorrelativo(1);

        var isValid = builder.Validate();
        var errors = builder.GetErrors();

        Assert.False(isValid);
        Assert.Contains("RUC de empresa inválido", errors);
    }

    [Fact]
    public void Validate_WithEmptyDetails_ReturnsFalse()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "20123456789", RazonSocial = "TEST" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENT" })
            .WithSerie("F001")
            .WithCorrelativo(1);

        var isValid = builder.Validate();

        Assert.False(isValid);
    }

    [Fact]
    public void Invoice_DefaultType_IsFactura()
    {
        var invoice = new Invoice();
        Assert.Equal(VoucherType.Factura, invoice.TipoDoc);
    }

    [Fact]
    public void CreditNote_DefaultType_IsNotaCredito()
    {
        var note = new CreditNote();
        Assert.Equal(VoucherType.NotaCredito, note.TipoDoc);
    }

    [Fact]
    public void DebitNote_DefaultType_IsNotaDebito()
    {
        var note = new DebitNote();
        Assert.Equal(VoucherType.NotaDebito, note.TipoDoc);
    }

    [Fact]
    public void Receipt_DefaultType_IsBoleta()
    {
        var receipt = new Receipt();
        Assert.Equal(VoucherType.Boleta, receipt.TipoDoc);
    }
}
