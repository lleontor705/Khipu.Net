namespace Khipu.Tests;

using Khipu.Core.Builder;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class InvoiceBuilderEdgeCaseTests
{
    private InvoiceBuilder CreateBaseBuilder() => (InvoiceBuilder)new InvoiceBuilder()
        .WithCompany(new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address { CodigoLocal = "0000" } })
        .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20000000001", RznSocial = "CLI" })
        .WithSerie("F001")
        .WithCorrelativo(1)
        .WithFechaEmision(DateTime.Today);

    [Fact]
    public void Build_WithIvapDetails_CalculatesIvapTotals()
    {
        var builder = CreateBaseBuilder();
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P1", Descripcion = "Producto IVAP", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 100, MtoValorVenta = 100,
            PrecioVenta = 104, TipoAfectacionIgv = TaxType.Ivap, TasaIgv = 0.04m
        });

        var invoice = builder.Build();
        Assert.True(invoice.MtoIvap > 0);
    }

    [Fact]
    public void Build_WithExoneradoDetails_SetsExoneradasTotal()
    {
        var builder = CreateBaseBuilder();
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P1", Descripcion = "Exonerado", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 100, MtoValorVenta = 100,
            PrecioVenta = 100, TipoAfectacionIgv = TaxType.Exonerado, TasaIgv = 0
        });

        var invoice = builder.Build();
        Assert.Equal(100m, invoice.MtoOperExoneradas);
        Assert.Equal(0m, invoice.MtoIGV);
    }

    [Fact]
    public void Build_WithGratuitoDetails_SetsGratuitasTotals()
    {
        var builder = CreateBaseBuilder();
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P1", Descripcion = "Gratuito", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 100, MtoValorVenta = 100,
            PrecioVenta = 0, TipoAfectacionIgv = TaxType.Gratuito, TasaIgv = 0.18m, MtoValorGratuito = 118
        });

        var invoice = builder.Build();
        Assert.Equal(100m, invoice.MtoOperGratuitas);
    }

    [Fact]
    public void Build_WithMultipleTaxTypes_CalculatesAll()
    {
        var builder = CreateBaseBuilder();
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P1", Descripcion = "Gravado", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 500, MtoValorVenta = 500,
            PrecioVenta = 590, TipoAfectacionIgv = TaxType.Gravado
        });
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P2", Descripcion = "Exonerado", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 200, MtoValorVenta = 200,
            PrecioVenta = 200, TipoAfectacionIgv = TaxType.Exonerado, TasaIgv = 0
        });

        var invoice = builder.Build();
        Assert.Equal(500m, invoice.MtoOperGravadas);
        Assert.Equal(200m, invoice.MtoOperExoneradas);
        Assert.True(invoice.MtoIGV > 0);
        Assert.True(invoice.MtoImpVenta > 0);
    }

    [Fact]
    public void Validate_WithFutureDate_ReturnsFalse()
    {
        var builder = (InvoiceBuilder)new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address() })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20000000001", RznSocial = "CLI" })
            .WithSerie("F001")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Today.AddDays(30));
        builder.AddDetail(new SaleDetail
        {
            Codigo = "P1", Descripcion = "Prod", Unidad = "NIU",
            Cantidad = 1, MtoValorUnitario = 100, MtoValorVenta = 100, PrecioVenta = 118
        });

        Assert.False(builder.Validate());
    }

    [Fact]
    public void Validate_WithEmptyDetails_ReturnsFalse()
    {
        var builder = (InvoiceBuilder)new InvoiceBuilder()
            .WithCompany(new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address() })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20000000001", RznSocial = "CLI" })
            .WithSerie("F001")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Today);

        Assert.False(builder.Validate());
    }
}
