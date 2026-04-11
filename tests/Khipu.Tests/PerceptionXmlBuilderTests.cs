namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class PerceptionXmlBuilderTests
{
    [Fact]
    public void Build_WithValidPerception_GeneratesValidXml()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.NotNull(xml);
        Assert.Contains("Perception", xml);
        Assert.Contains("P001-1", xml);
    }

    [Fact]
    public void Build_ContainsAgentAndReceiverParty()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.Contains("AgentParty", xml);
        Assert.Contains("ReceiverParty", xml);
        Assert.Contains("20123456789", xml);
        Assert.Contains("PROVEEDOR SRL", xml);
    }

    [Fact]
    public void Build_ContainsPerceptionSystemCode()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.Contains("SUNATPerceptionSystemCode", xml);
        Assert.Contains("SUNATPerceptionPercent", xml);
    }

    [Fact]
    public void Build_ContainsTotals()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.Contains("TotalInvoiceAmount", xml);
        Assert.Contains("SUNATTotalCashed", xml);
        Assert.Contains("PEN", xml);
    }

    [Fact]
    public void Build_ContainsDocumentReferences()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.Contains("SUNATPerceptionDocumentReference", xml);
        Assert.Contains("F001-100", xml);
        Assert.Contains("SUNATPerceptionInformation", xml);
        Assert.Contains("SUNATPerceptionAmount", xml);
        Assert.Contains("SUNATNetTotalCashed", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var fileName = builder.GetFileName(perception);

        Assert.Equal("20123456789-40-P001-00000001.xml", fileName);
    }

    [Fact]
    public void Build_ContainsCorrectNamespaces()
    {
        var perception = CreateTestPerception();
        var builder = new PerceptionXmlBuilder();

        var xml = builder.Build(perception);

        Assert.Contains("urn:sunat:names:specification:ubl:peru:schema:xsd:Perception-1", xml);
        Assert.Contains("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1", xml);
    }

    private Perception CreateTestPerception()
    {
        return new Perception
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
            Proveedor = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "PROVEEDOR SRL"
            },
            Serie = "P001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            MtoPercepcion = 23.60m,
            MtoTotal = 1180.00m,
            MtoTotalCobrar = 1203.60m,
            Details = new List<PerceptionDetail>
            {
                new()
                {
                    Orden = 1,
                    TipoDoc = "01",
                    NumDoc = "F001-100",
                    FechaEmision = new DateTime(2026, 4, 5),
                    CodMoneda = "PEN",
                    ImpTotal = 1180.00m,
                    ImpCobrar = 1180.00m,
                    CodReg = "01", // Venta interna - 2%
                    Porcentaje = 2.00m,
                    MtoBase = 1180.00m,
                    Mto = 23.60m
                }
            }
        };
    }
}
