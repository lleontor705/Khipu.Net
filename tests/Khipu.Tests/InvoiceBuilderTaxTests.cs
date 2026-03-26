namespace Khipu.Tests;

using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class InvoiceBuilderTaxTests
{
    [Fact]
    public void Build_WithMixedTaxTypes_CalculatesCorrectTotals()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = \"20123456789\", RazonSocial = \"TEST\" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENT\" })
            .WithSerie(\"F001\")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now)
            .AddDetail(new SaleDetail 
            { 
                Cantidad = 1, 
                MtoValorUnitario = 100, 
                MtoValorVenta = 100,
                TipoAfectacionIgv = TaxType.Gravado,
                TasaIgv = 0.18m
            })
            .AddDetail(new SaleDetail 
            { 
                Cantidad = 1, 
                MtoValorUnitario = 50, 
                MtoValorVenta = 50,
                TipoAfectacionIgv = TaxType.Exonerado
            })
            .AddDetail(new SaleDetail 
            { 
                Cantidad = 1, 
                MtoValorUnitario = 30, 
                MtoValorVenta = 30,
                TipoAfectacionIgv = TaxType.Inafecto
            });

        var invoice = builder.Build();

        Assert.Equal(100, invoice.MtoOperGravadas);
        Assert.Equal(50, invoice.MtoOperExoneradas);
        Assert.Equal(30, invoice.MtoOperInafectas);
        Assert.Equal(18, invoice.MtoIGV); // 18% de 100
        Assert.Equal(198, invoice.MtoImpVenta); // 100 + 50 + 30 + 18
    }

    [Fact]
    public void Build_WithExportOperation_CalculatesCorrectTotals()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = \"20123456789\", RazonSocial = \"TEST\" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENT\" })
            .WithSerie(\"F001\")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now)
            .AddDetail(new SaleDetail 
            { 
                Cantidad = 1, 
                MtoValorUnitario = 1000, 
                MtoValorVenta = 1000,
                TipoAfectacionIgv = TaxType.Exportacion
            });

        var invoice = builder.Build();

        Assert.Equal(1000, invoice.MtoOperExportacion);
        Assert.Equal(0, invoice.MtoIGV); // Exportación no tiene IGV
        Assert.Equal(1000, invoice.MtoImpVenta);
    }

    [Fact]
    public void Build_WithISC_CalculatesCorrectTotals()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = \"20123456789\", RazonSocial = \"TEST\" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENT\" })
            .WithSerie(\"F001\")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now)
            .AddDetail(new SaleDetail 
            { 
                Cantidad = 1, 
                MtoValorUnitario = 100, 
                MtoValorVenta = 100,
                TipoAfectacionIgv = TaxType.Gravado,
                TasaIgv = 0.18m,
                MtoIsc = 10
            });

        var invoice = builder.Build();

        Assert.Equal(100, invoice.MtoOperGravadas);
        Assert.Equal(18, invoice.MtoIGV);
        Assert.Equal(10, invoice.MtoISC);
        Assert.Equal(128, invoice.MtoImpVenta); // 100 + 18 + 10
    }

    [Fact]
    public void Build_WithFutureDate_ValidationFails()
    {
        var builder = new InvoiceBuilder()
            .WithCompany(new Company { Ruc = \"20123456789\", RazonSocial = \"TEST\" })
            .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENT\" })
            .WithSerie(\"F001\")
            .WithCorrelativo(1)
            .WithFechaEmision(DateTime.Now.AddDays(1));

        var isValid = builder.Validate();

        Assert.False(isValid);
        Assert.Contains(\"futura\", builder.GetErrors().First().ToLower());
    }
}
