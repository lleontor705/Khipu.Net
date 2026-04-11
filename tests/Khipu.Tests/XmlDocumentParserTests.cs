namespace Khipu.Tests;

using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Khipu.Xml.Parser;
using Xunit;

public class XmlDocumentParserTests
{
    [Fact]
    public void ParseInvoice_WithGeneratedXml_RoundTrips()
    {
        var original = TestDataFactory.CreateInvoice();
        var builder = new InvoiceXmlBuilder();
        var xml = builder.Build(original);

        var parsed = XmlDocumentParser.ParseInvoice(xml);

        Assert.NotNull(parsed);
        Assert.Equal("F001", parsed!.Serie);
        Assert.Equal(1, parsed.Correlativo);
        Assert.Equal("20123456789", parsed.Company.Ruc);
        Assert.Equal("EMPRESA SAC", parsed.Company.RazonSocial);
        Assert.Equal("20987654321", parsed.Client.NumDoc);
        Assert.Equal(Currency.Pen, parsed.Moneda);
        Assert.Equal(118m, parsed.MtoImpVenta);
        Assert.Single(parsed.Details);
        Assert.Equal("PROD001", parsed.Details[0].Codigo);
    }

    [Fact]
    public void ParseCreditNote_WithGeneratedXml_RoundTrips()
    {
        var original = TestDataFactory.CreateCreditNote();
        var builder = new CreditNoteXmlBuilder();
        var xml = builder.Build(original);

        var parsed = XmlDocumentParser.ParseCreditNote(xml);

        Assert.NotNull(parsed);
        Assert.Equal("FC01", parsed!.Serie);
        Assert.Equal(1, parsed.Correlativo);
        Assert.Equal("F001-100", parsed.NumDocAfectado);
        Assert.Equal("01", parsed.CodMotivo);
        Assert.Equal(59m, parsed.MtoImpVenta);
    }

    [Fact]
    public void ParseDebitNote_WithGeneratedXml_RoundTrips()
    {
        var original = TestDataFactory.CreateDebitNote();
        var builder = new DebitNoteXmlBuilder();
        var xml = builder.Build(original);

        var parsed = XmlDocumentParser.ParseDebitNote(xml);

        Assert.NotNull(parsed);
        Assert.Equal("FD01", parsed!.Serie);
        Assert.Equal("F001-100", parsed.NumDocAfectado);
    }

    [Fact]
    public void ParseDespatch_WithGeneratedXml_RoundTrips()
    {
        var original = TestDataFactory.CreateDespatch();
        var builder = new DespatchXmlBuilder();
        var xml = builder.Build(original);

        var parsed = XmlDocumentParser.ParseDespatch(xml);

        Assert.NotNull(parsed);
        Assert.Equal("T001", parsed!.Serie);
        Assert.Equal(1, parsed.Correlativo);
        Assert.Equal("01", parsed.CodMotivoTraslado);
        Assert.Single(parsed.Details);
        Assert.Equal("PROD001", parsed.Details[0].Codigo);
    }

    [Fact]
    public void ParseInvoice_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParseInvoice(null!));
        Assert.Null(XmlDocumentParser.ParseInvoice(""));
        Assert.Null(XmlDocumentParser.ParseInvoice("<bad xml"));
    }

    [Fact]
    public void ParseInvoice_ExtractsTaxTotals()
    {
        var original = TestDataFactory.CreateInvoice();
        var builder = new InvoiceXmlBuilder();
        var xml = builder.Build(original);

        var parsed = XmlDocumentParser.ParseInvoice(xml);

        Assert.NotNull(parsed);
        // TotalImpuestos comes from the TaxTotal/TaxAmount element
        Assert.Equal(original.TotalImpuestos > 0 ? original.TotalImpuestos
            : original.MtoIGV + original.MtoISC + original.MtoIvap + original.MtoOtrosTributos + original.Icbper,
            parsed!.TotalImpuestos);
    }
}

/// <summary>
/// Factory compartida de datos de prueba para tests.
/// </summary>
internal static class TestDataFactory
{
    internal static Khipu.Data.Documents.Invoice CreateInvoice()
    {
        return new Khipu.Data.Documents.Invoice
        {
            Company = CreateCompany(),
            Client = CreateClient(),
            Serie = "F001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            MtoOperGravadas = 100,
            MtoIGV = 18,
            MtoImpVenta = 118,
            Details = new()
            {
                new()
                {
                    Codigo = "PROD001", Descripcion = "Producto", Unidad = "NIU",
                    Cantidad = 1, MtoValorUnitario = 100, MtoValorVenta = 100, PrecioVenta = 118
                }
            }
        };
    }

    internal static Khipu.Data.Documents.CreditNote CreateCreditNote()
    {
        return new Khipu.Data.Documents.CreditNote
        {
            Company = CreateCompany(),
            Client = CreateClient(),
            Serie = "FC01", Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            TipDocAfectado = "01", NumDocAfectado = "F001-100",
            CodMotivo = "01", DesMotivo = "Anulacion",
            MtoOperGravadas = 50, MtoIGV = 9, MtoImpVenta = 59,
            Details = new()
            {
                new()
                {
                    Codigo = "PROD001", Descripcion = "Devolucion", Unidad = "NIU",
                    Cantidad = 1, MtoValorUnitario = 50, MtoValorVenta = 50, PrecioVenta = 59
                }
            }
        };
    }

    internal static Khipu.Data.Documents.DebitNote CreateDebitNote()
    {
        return new Khipu.Data.Documents.DebitNote
        {
            Company = CreateCompany(),
            Client = CreateClient(),
            Serie = "FD01", Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            Moneda = Currency.Pen,
            TipDocAfectado = "01", NumDocAfectado = "F001-100",
            CodMotivo = "02", DesMotivo = "Aumento de valor",
            MtoOperGravadas = 50, MtoIGV = 9, MtoImpVenta = 59,
            Details = new()
            {
                new()
                {
                    Codigo = "PROD001", Descripcion = "Ajuste", Unidad = "NIU",
                    Cantidad = 1, MtoValorUnitario = 50, MtoValorVenta = 50, PrecioVenta = 59
                }
            }
        };
    }

    internal static Khipu.Data.Documents.Despatch CreateDespatch()
    {
        return new Khipu.Data.Documents.Despatch
        {
            Company = CreateCompany(),
            Destinatario = CreateClient(),
            Serie = "T001", Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            CodMotivoTraslado = "01", DesMotivoTraslado = "Venta",
            PesoTotal = 25.5m, UndPesoTotal = "KGM",
            PuntoPartida = new Khipu.Data.Entities.Address { Ubigeo = "150101", Direccion = "Origen" },
            PuntoLlegada = new Khipu.Data.Entities.Address { Ubigeo = "150201", Direccion = "Destino" },
            Details = new()
            {
                new() { Codigo = "PROD001", Descripcion = "Producto", Unidad = "NIU", Cantidad = 10 }
            }
        };
    }

    private static Khipu.Data.Entities.Company CreateCompany() => new()
    {
        Ruc = "20123456789",
        RazonSocial = "EMPRESA SAC",
        Address = new Khipu.Data.Entities.Address
        {
            Ubigeo = "150101", Departamento = "LIMA", Provincia = "LIMA",
            Distrito = "LIMA", Direccion = "AV. PRINCIPAL 123", CodigoLocal = "0000"
        }
    };

    private static Khipu.Data.Entities.Client CreateClient() => new()
    {
        TipoDoc = Khipu.Data.Enums.DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SRL"
    };
}
