namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class ReceiptXmlBuilderTests
{
    [Fact]
    public void Build_WithValidReceipt_GeneratesValidXml()
    {
        var receipt = CreateTestReceipt();
        var builder = new ReceiptXmlBuilder();

        var xml = builder.Build(receipt);

        Assert.NotNull(xml);
        Assert.Contains("Invoice", xml);
        Assert.Contains("B001-123", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var receipt = CreateTestReceipt();
        var builder = new ReceiptXmlBuilder();

        var fileName = builder.GetFileName(receipt);

        Assert.Equal("20123456789-03-B001-00000123.xml", fileName);
    }

    private Receipt CreateTestReceipt()
    {
        return new Receipt
        {
            Company = new Company
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
            },
            Client = new Client
            {
                TipoDoc = DocumentType.Dni,
                NumDoc = "12345678",
                RznSocial = "JUAN PEREZ"
            },
            Serie = "B001",
            Correlativo = 123,
            FechaEmision = new DateTime(2026, 3, 26),
            Moneda = Currency.Pen,
            MtoOperGravadas = 100,
            MtoIGV = 18,
            MtoImpVenta = 118,
            Details = new List<SaleDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Producto",
                    Unidad = "NIU",
                    Cantidad = 1,
                    MtoValorUnitario = 100,
                    MtoValorVenta = 100,
                    PrecioVenta = 118
                }
            }
        };
    }
}

public class DebitNoteXmlBuilderTests
{
    [Fact]
    public void Build_WithValidDebitNote_GeneratesValidXml()
    {
        var note = CreateTestDebitNote();
        var builder = new DebitNoteXmlBuilder();

        var xml = builder.Build(note);

        Assert.NotNull(xml);
        Assert.Contains("DebitNote", xml);
        Assert.Contains("DiscrepancyResponse", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var note = CreateTestDebitNote();
        var builder = new DebitNoteXmlBuilder();

        var fileName = builder.GetFileName(note);

        Assert.Equal("20123456789-08-FD01-00000123.xml", fileName);
    }

    private DebitNote CreateTestDebitNote()
    {
        return new DebitNote
        {
            Company = new Company
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
            },
            Client = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL"
            },
            Serie = "FD01",
            Correlativo = 123,
            FechaEmision = new DateTime(2026, 3, 26),
            Moneda = Currency.Pen,
            TipDocAfectado = "01",
            NumDocAfectado = "F001-100",
            CodMotivo = "02",
            DesMotivo = "Aumento de valor",
            MtoOperGravadas = 50,
            MtoIGV = 9,
            MtoImpVenta = 59,
            Details = new List<SaleDetail>
            {
                new()
                {
                    Codigo = "PROD001",
                    Descripcion = "Ajuste",
                    Unidad = "NIU",
                    Cantidad = 1,
                    MtoValorUnitario = 50,
                    MtoValorVenta = 50,
                    PrecioVenta = 59
                }
            }
        };
    }
}
