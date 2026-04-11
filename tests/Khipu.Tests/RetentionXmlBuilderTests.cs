namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class RetentionXmlBuilderTests
{
    [Fact]
    public void Build_WithValidRetention_GeneratesValidXml()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.NotNull(xml);
        Assert.Contains("Retention", xml);
        Assert.Contains("R001-1", xml);
    }

    [Fact]
    public void Build_ContainsAgentAndReceiverParty()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("AgentParty", xml);
        Assert.Contains("ReceiverParty", xml);
        Assert.Contains("20123456789", xml);
        Assert.Contains("PROVEEDOR SRL", xml);
    }

    [Fact]
    public void Build_ContainsRetentionSystemCode()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("SUNATRetentionSystemCode", xml);
        Assert.Contains("SUNATRetentionPercent", xml);
    }

    [Fact]
    public void Build_ContainsTotals()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("TotalInvoiceAmount", xml);
        Assert.Contains("SUNATTotalPaid", xml);
        Assert.Contains("PEN", xml);
    }

    [Fact]
    public void Build_ContainsDocumentReferences()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("SUNATRetentionDocumentReference", xml);
        Assert.Contains("F001-200", xml);
        Assert.Contains("SUNATRetentionInformation", xml);
        Assert.Contains("SUNATRetentionAmount", xml);
        Assert.Contains("SUNATNetTotalPaid", xml);
    }

    [Fact]
    public void Build_ContainsPayments()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("Payment", xml);
        Assert.Contains("PaidAmount", xml);
        Assert.Contains("PaidDate", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var fileName = builder.GetFileName(retention);

        Assert.Equal("20123456789-20-R001-00000001.xml", fileName);
    }

    [Fact]
    public void Build_ContainsCorrectNamespaces()
    {
        var retention = CreateTestRetention();
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        Assert.Contains("urn:sunat:names:specification:ubl:peru:schema:xsd:Retention-1", xml);
        Assert.Contains("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1", xml);
    }

    [Fact]
    public void Build_WithMultiplePayments_ContainsAllPayments()
    {
        var retention = CreateTestRetention();
        retention.Details[0].Pagos!.Add(new Payment
        {
            FormaPago = "001",
            Monto = 500.00m
        });
        var builder = new RetentionXmlBuilder();

        var xml = builder.Build(retention);

        // Should contain payment IDs 1 and 2
        Assert.Contains("<cbc:ID>1</cbc:ID>", xml);
        Assert.Contains("<cbc:ID>2</cbc:ID>", xml);
    }

    private Retention CreateTestRetention()
    {
        return new Retention
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
            Serie = "R001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            MtoRetencion = 35.40m,
            MtoTotal = 1180.00m,
            Details = new List<RetentionDetail>
            {
                new()
                {
                    Orden = 1,
                    TipoDoc = "01",
                    NumDoc = "F001-200",
                    FechaEmision = new DateTime(2026, 4, 1),
                    FechaPago = new DateTime(2026, 4, 10),
                    ImpTotal = 1180.00m,
                    ImpPagar = 1144.60m,
                    CodMoneda = "PEN",
                    Pagos = new List<Payment>
                    {
                        new()
                        {
                            FormaPago = "001",
                            Monto = 1180.00m
                        }
                    }
                }
            }
        };
    }
}
