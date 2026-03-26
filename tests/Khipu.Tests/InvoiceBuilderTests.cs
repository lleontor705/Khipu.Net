namespace Khipu.Tests;

using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class InvoiceBuilderTests
{
    [Fact]
    public void BuildInvoice_WithValidData_ReturnsInvoice()
    {
        // Arrange
        var company = new Company
        {
            Ruc = "20123456789",
            RazonSocial = "EMPRESA SAC"
        };
        
        var client = new Client
        {
            TipoDoc = DocumentType.Ruc,
            NumDoc = "20987654321",
            RznSocial = "CLIENTE SRL"
        };
        
        var detail = new SaleDetail
        {
            Orden = 1,
            Codigo = "PROD001",
            Descripcion = "Producto de prueba",
            Cantidad = 2,
            MtoValorUnitario = 100,
            MtoValorVenta = 200
        };
        
        var builder = new InvoiceBuilder()
            .WithCompany(company)
            .WithClient(client)
            .WithSerie("F001")
            .WithCorrelativo(123)
            .WithFechaEmision(DateTime.Now)
            .AddDetail(detail);

        // Act
        var invoice = builder.Build();

        // Assert
        Assert.NotNull(invoice);
        Assert.Equal("F001", invoice.Serie);
        Assert.Equal(123, invoice.Correlativo);
        Assert.Equal(VoucherType.Factura, invoice.TipoDoc);
        Assert.Single(invoice.Details);
        Assert.Equal(200, invoice.MtoOperGravadas);
        Assert.Equal(36, invoice.MtoIGV); // 18% of 200
    }

    [Fact]
    public void ValidateInvoice_WithMissingData_ReturnsFalse()
    {
        // Arrange
        var builder = new InvoiceBuilder()
            .WithSerie("F001")
            .WithCorrelativo(0); // Missing required data

        // Act
        var isValid = builder.Validate();

        // Assert
        Assert.False(isValid);
    }
}
