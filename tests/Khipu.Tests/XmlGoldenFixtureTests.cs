namespace Khipu.Tests;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Tests.Support;
using Khipu.Xml.Builder;

public class XmlGoldenFixtureTests
{
    [Fact]
    public void InvoiceXml_MatchesGoldenFixtureWithXPathDiagnostics()
    {
        var invoice = BuildInvoice();
        var xml = new InvoiceXmlBuilder().Build(invoice);
        var fixture = ReadFixture("golden/invoice.golden.json");

        var comparison = XPathGoldenComparator.CompareDetailed(xml, fixture);

        Assert.True(comparison.UnexpectedDeltas.Count == 0, string.Join(Environment.NewLine, comparison.UnexpectedDeltas));
        Assert.True(comparison.Errors.Count == 0, string.Join(Environment.NewLine, comparison.Errors));
    }

    [Fact]
    public void SummaryXml_MatchesGoldenFixtureWithXPathDiagnostics()
    {
        var summary = BuildSummary();
        var xml = new SummaryXmlBuilder().Build(summary);
        var fixture = ReadFixture("golden/summary.golden.json");

        var comparison = XPathGoldenComparator.CompareDetailed(xml, fixture);

        Assert.True(comparison.UnexpectedDeltas.Count == 0, string.Join(Environment.NewLine, comparison.UnexpectedDeltas));
        Assert.True(comparison.Errors.Count == 0, string.Join(Environment.NewLine, comparison.Errors));
    }

    [Fact]
    public void VoidedXml_MatchesGoldenFixtureWithXPathDiagnostics()
    {
        var voided = BuildVoided();
        var xml = new VoidedXmlBuilder().Build(voided);
        var fixture = ReadFixture("golden/voided.golden.json");

        var comparison = XPathGoldenComparator.CompareDetailed(xml, fixture);

        Assert.True(comparison.UnexpectedDeltas.Count == 0, string.Join(Environment.NewLine, comparison.UnexpectedDeltas));
        Assert.True(comparison.Errors.Count == 0, string.Join(Environment.NewLine, comparison.Errors));
    }

    private static string ReadFixture(string relativePath)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    private static Invoice BuildInvoice() => new()
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
            Address = new Address { Direccion = "JR. SECUNDARIO 456" }
        },
        Serie = "F001",
        Correlativo = 123,
        FechaEmision = new DateTime(2026, 3, 26, 12, 30, 0),
        Moneda = Currency.Pen,
        MtoOperGravadas = 200,
        MtoIGV = 36,
        MtoImpVenta = 236,
        Leyendas = [new Legend { Code = "1000", Value = "SON DOSCIENTOS TREINTA Y SEIS CON 00/100 SOLES" }],
        Details = [new SaleDetail { Codigo = "PROD001", Descripcion = "Producto de prueba", Unidad = "NIU", Cantidad = 2, MtoValorUnitario = 100, MtoValorVenta = 200, PrecioVenta = 236 }]
    };

    private static Summary BuildSummary() => new()
    {
        Company = new Company { Ruc = "20123456789", RazonSocial = "EMPRESA SAC" },
        Correlativo = "001",
        FechaGeneracion = new DateTime(2026, 3, 26),
        FechaEnvio = new DateTime(2026, 3, 27),
        Details = [new SummaryDetail { TipoDoc = VoucherType.Boleta, SerieNro = "B001-001", ClienteTipoDoc = "1", ClienteNroDoc = "12345678", MtoOperGravadas = 100, MtoIGV = 18, MtoImpVenta = 118 }]
    };

    private static Voided BuildVoided() => new()
    {
        Company = new Company { Ruc = "20123456789", RazonSocial = "EMPRESA SAC" },
        Correlativo = "001",
        FechaGeneracion = new DateTime(2026, 3, 26),
        FechaEnvio = new DateTime(2026, 3, 27),
        Details = [new VoidedDetail { TipoDoc = "01", SerieNro = "F001-001", FechaDoc = new DateTime(2026, 3, 25), MotivoBaja = "Error de emisión" }]
    };
}
