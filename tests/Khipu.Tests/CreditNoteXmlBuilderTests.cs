namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class CreditNoteXmlBuilderTests
{
    [Fact]
    public void Build_WithValidCreditNote_GeneratesValidXml()
    {
        var note = CreateTestCreditNote();
        var builder = new CreditNoteXmlBuilder();

        var xml = builder.Build(note);

        Assert.NotNull(xml);
        Assert.Contains("CreditNote", xml);
        Assert.Contains("DiscrepancyResponse", xml);
        Assert.Contains("BillingReference", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var note = CreateTestCreditNote();
        var builder = new CreditNoteXmlBuilder();

        var fileName = builder.GetFileName(note);

        Assert.Equal("20123456789-07-FC01-00000123.xml", fileName);
    }

    [Fact]
    public void Build_ContainsMotivoInfo()
    {
        var note = CreateTestCreditNote();
        var builder = new CreditNoteXmlBuilder();

        var xml = builder.Build(note);

        Assert.Contains("F001-100", xml); // NumDocAfectado
        Assert.Contains("01", xml); // CodMotivo
        Assert.Contains("Anulación", xml); // DesMotivo
    }

    private CreditNote CreateTestCreditNote()
    {
        return new CreditNote
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
                RznSocial = "CLIENTE SRL",
                Address = new Address { Direccion = "JR. SECUNDARIO 456" }
            },
            Serie = "FC01",
            Correlativo = 123,
            FechaEmision = new DateTime(2026, 3, 26),
            Moneda = Currency.Pen,
            TipDocAfectado = "01",
            NumDocAfectado = "F001-100",
            CodMotivo = "01",
            DesMotivo = "Anulación de la operación",
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
}
