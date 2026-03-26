namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class SummaryXmlBuilderTests
{
    [Fact]
    public void Build_WithValidSummary_GeneratesValidXml()
    {
        var summary = CreateTestSummary();
        var builder = new SummaryXmlBuilder();

        var xml = builder.Build(summary);

        Assert.NotNull(xml);
        Assert.Contains("SummaryDocuments", xml);
        Assert.Contains("001", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var summary = CreateTestSummary();
        var builder = new SummaryXmlBuilder();

        var fileName = builder.GetFileName(summary);

        Assert.Contains("-RC-", fileName);
        Assert.EndsWith(".xml", fileName);
    }

    private Summary CreateTestSummary()
    {
        return new Summary
        {
            Company = new Company
            {
                Ruc = "20123456789",
                RazonSocial = "EMPRESA SAC"
            },
            Correlativo = "001",
            FechaGeneracion = new DateTime(2026, 3, 26),
            FechaEnvio = new DateTime(2026, 3, 27),
            Details = new List<SummaryDetail>
            {
                new()
                {
                    TipoDoc = VoucherType.Boleta,
                    SerieNro = "B001-001",
                    ClienteTipoDoc = "1",
                    ClienteNroDoc = "12345678",
                    MtoOperGravadas = 100,
                    MtoIGV = 18,
                    MtoImpVenta = 118
                }
            }
        };
    }
}

public class VoidedXmlBuilderTests
{
    [Fact]
    public void Build_WithValidVoided_GeneratesValidXml()
    {
        var voided = CreateTestVoided();
        var builder = new VoidedXmlBuilder();

        var xml = builder.Build(voided);

        Assert.NotNull(xml);
        Assert.Contains("VoidedDocuments", xml);
        Assert.Contains("001", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var voided = CreateTestVoided();
        var builder = new VoidedXmlBuilder();

        var fileName = builder.GetFileName(voided);

        Assert.Contains("-RA-", fileName);
        Assert.EndsWith(".xml", fileName);
    }

    private Voided CreateTestVoided()
    {
        return new Voided
        {
            Company = new Company
            {
                Ruc = "20123456789",
                RazonSocial = "EMPRESA SAC"
            },
            Correlativo = "001",
            FechaGeneracion = new DateTime(2026, 3, 26),
            FechaEnvio = new DateTime(2026, 3, 27),
            Details = new List<VoidedDetail>
            {
                new()
                {
                    TipoDoc = "01",
                    SerieNro = "F001-001",
                    FechaDoc = new DateTime(2026, 3, 25),
                    MotivoBaja = "Error de emisión"
                }
            }
        };
    }
}
