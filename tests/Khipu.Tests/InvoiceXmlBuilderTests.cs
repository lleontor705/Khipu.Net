namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class InvoiceXmlBuilderTests
{
    [Fact]
    public void Build_WithValidInvoice_GeneratesValidXml()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var xml = builder.Build(invoice);

        // Assert
        Assert.NotNull(xml);
        Assert.Contains("Invoice", xml);
        Assert.Contains("xmlns", xml);
        Assert.Contains("2.1", xml);
        Assert.Contains("F001-123", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var fileName = builder.GetFileName(invoice);

        // Assert
        Assert.Equal("20123456789-01-F001-00000123.xml", fileName);
    }

    [Fact]
    public void Build_ContainsCompanyInfo()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var xml = builder.Build(invoice);

        // Assert
        Assert.Contains("20123456789", xml);
        Assert.Contains("EMPRESA SAC", xml);
    }

    [Fact]
    public void Build_ContainsClientInfo()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var xml = builder.Build(invoice);

        // Assert
        Assert.Contains("20987654321", xml);
        Assert.Contains("CLIENTE SRL", xml);
    }

    [Fact]
    public void Build_ContainsTotals()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var xml = builder.Build(invoice);

        // Assert
        Assert.Contains("LineExtensionAmount", xml);
        Assert.Contains("TaxTotal", xml);
        Assert.Contains("PayableAmount", xml);
    }

    [Fact]
    public void Build_ContainsDetails()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var builder = new InvoiceXmlBuilder();

        // Act
        var xml = builder.Build(invoice);

        // Assert
        Assert.Contains("InvoiceLine", xml);
        Assert.Contains("PROD001", xml);
        Assert.Contains("Producto de prueba", xml);
    }

    private Invoice CreateTestInvoice()
    {
        return new Invoice
        {
            Company = new Company
            {
                Ruc = "20123456789",
                RazonSocial = "EMPRESA SAC",
                NombreComercial = "EMPRESA",
                Address = new Address
                {
                    Ubigeo = "150101",
                    Departamento = "LIMA",
                    Provincia = "LIMA",
                    Distrito = "LIMA",
                    Direccion = "AV. PRINCIPAL 123",
                    CodigoLocal = "0000"
                }
            },
            Client = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL",
                Address = new Address
                {
                    Direccion = "JR. SECUNDARIO 456"
                }
            },
            Serie = "F001",
            Correlativo = 123,
            FechaEmision = new DateTime(2026, 3, 26, 12, 30, 0),
            Moneda = Currency.Pen,
            MtoOperGravadas = 200,
            MtoIGV = 36,
            MtoImpVenta = 236,
            Details = new List<SaleDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Producto de prueba",
                    Unidad = "NIU",
                    Cantidad = 2,
                    MtoValorUnitario = 100,
                    MtoValorVenta = 200,
                    PrecioVenta = 236
                }
            }
        };
    }
}
