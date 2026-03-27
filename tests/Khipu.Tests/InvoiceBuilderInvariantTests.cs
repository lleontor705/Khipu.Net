namespace Khipu.Tests;

using Khipu.Core.Builder;
using Khipu.Core.Interfaces;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

public class InvoiceBuilderInvariantTests
{
    [Fact]
    public void Build_WithMixedTaxes_SatisfiesInvoiceAggregateInvariants()
    {
        var invoice = NewBuilder()
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 100m, MtoValorVenta = 100m, TipoAfectacionIgv = TaxType.Gravado, MtoIsc = 5m, OtroTributo = 1m })
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 50m, MtoValorVenta = 50m, TipoAfectacionIgv = TaxType.Exonerado })
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 30m, MtoValorVenta = 30m, TipoAfectacionIgv = TaxType.Inafecto })
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 20m, MtoValorVenta = 20m, TipoAfectacionIgv = TaxType.Exportacion })
            .Build();

        // Greenter invariant: TotalImpuestos = IGV + ISC + IVAP + OtrosTributos + ICBPER
        var expectedTotalImpuestos = invoice.MtoIGV + invoice.MtoISC + invoice.MtoIvap + invoice.MtoOtrosTributos + invoice.Icbper;
        var expectedMtoImpVenta = invoice.MtoOperGravadas + invoice.MtoOperExoneradas + invoice.MtoOperInafectas + invoice.MtoOperExportacion + invoice.TotalImpuestos;

        Assert.Equal(expectedTotalImpuestos, invoice.TotalImpuestos);
        Assert.Equal(expectedMtoImpVenta, invoice.MtoImpVenta);
        Assert.Equal(18m, invoice.MtoIGV);
        Assert.Equal(5m, invoice.MtoISC);
        Assert.Equal(1m, invoice.MtoOtrosTributos);
    }

    [Fact]
    public void Build_WhenComposedInDifferentDetailOrder_ProducesSameAggregates()
    {
        var detailFactories = new Func<SaleDetail>[]
        {
            static () => new SaleDetail { Cantidad = 1, MtoValorUnitario = 200m, MtoValorVenta = 200m, TipoAfectacionIgv = TaxType.Gravado },
            static () => new SaleDetail { Cantidad = 1, MtoValorUnitario = 80m, MtoValorVenta = 80m, TipoAfectacionIgv = TaxType.Exonerado },
            static () => new SaleDetail { Cantidad = 1, MtoValorUnitario = 20m, MtoValorVenta = 20m, TipoAfectacionIgv = TaxType.Inafecto }
        };

        var invoiceA = NewBuilder()
            .AddDetail(detailFactories[0]())
            .AddDetail(detailFactories[1]())
            .AddDetail(detailFactories[2]())
            .Build();

        var invoiceB = NewBuilder()
            .AddDetail(detailFactories[2]())
            .AddDetail(detailFactories[0]())
            .AddDetail(detailFactories[1]())
            .Build();

        Assert.Equal(invoiceA.MtoOperGravadas, invoiceB.MtoOperGravadas);
        Assert.Equal(invoiceA.MtoOperExoneradas, invoiceB.MtoOperExoneradas);
        Assert.Equal(invoiceA.MtoOperInafectas, invoiceB.MtoOperInafectas);
        Assert.Equal(invoiceA.MtoIGV, invoiceB.MtoIGV);
        Assert.Equal(invoiceA.TotalImpuestos, invoiceB.TotalImpuestos);
        Assert.Equal(invoiceA.MtoImpVenta, invoiceB.MtoImpVenta);
    }

    [Fact]
    public void Build_ComputesTotalsBeforeDefaultLegendComposition()
    {
        var invoice = NewBuilder()
            .AddDetail(new SaleDetail { Cantidad = 1, MtoValorUnitario = 100m, MtoValorVenta = 100m, TipoAfectacionIgv = TaxType.Gravado })
            .Build();

        Assert.True(invoice.MtoImpVenta > 0);
        var amountLegend = Assert.Single(invoice.Leyendas!, legend => legend.Code == "1000");
        Assert.False(string.IsNullOrWhiteSpace(amountLegend.Value));
    }

    private static IInvoiceBuilder NewBuilder()
    {
        return new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "20123456789", RazonSocial = "TEST" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENT" })
            .WithSerie("F001")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now);
    }
}
