namespace Khipu.Tests;

using Khipu.Data.Generators;
using Khipu.Xml.Builder;
using Khipu.Xml.Parser;
using Xunit;

public class XmlParserFullTests
{
    // ===== Parse methods not previously tested =====

    [Fact]
    public void ParseDebitNote_RoundTrips()
    {
        var original = DocumentGenerator.CreateDebitNote();
        var xml = new DebitNoteXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParseDebitNote(xml);

        Assert.NotNull(parsed);
        Assert.Equal("FD01", parsed!.Serie);
        Assert.Equal("02", parsed.CodMotivo);
        Assert.Equal("F001-00000001", parsed.NumDocAfectado);
    }

    [Fact]
    public void ParseDebitNote_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParseDebitNote(null!));
        Assert.Null(XmlDocumentParser.ParseDebitNote(""));
        Assert.Null(XmlDocumentParser.ParseDebitNote("<bad"));
    }

    [Fact]
    public void ParseDespatch_RoundTrips_WithTransport()
    {
        var original = DocumentGenerator.CreateDespatch();
        var xml = new DespatchXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParseDespatch(xml);

        Assert.NotNull(parsed);
        Assert.Equal("T001", parsed!.Serie);
        Assert.Equal("01", parsed.CodMotivoTraslado);
        Assert.Equal(2, parsed.Details.Count);
        Assert.NotNull(parsed.PuntoPartida);
        Assert.NotNull(parsed.PuntoLlegada);
    }

    [Fact]
    public void ParsePerception_RoundTrips()
    {
        var original = DocumentGenerator.CreatePerception();
        var xml = new PerceptionXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParsePerception(xml);

        Assert.NotNull(parsed);
        Assert.Equal("P001", parsed!.Serie);
        Assert.Equal(23.60m, parsed.MtoPercepcion);
        Assert.Single(parsed.Details);
        Assert.Equal("F001-00000001", parsed.Details[0].NumDoc);
    }

    [Fact]
    public void ParseRetention_RoundTrips()
    {
        var original = DocumentGenerator.CreateRetention();
        var xml = new RetentionXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParseRetention(xml);

        Assert.NotNull(parsed);
        Assert.Equal("R001", parsed!.Serie);
        Assert.Equal(35.40m, parsed.MtoRetencion);
        Assert.Single(parsed.Details);
    }

    [Fact]
    public void ParseSummary_RoundTrips()
    {
        var original = DocumentGenerator.CreateSummary();
        var xml = new SummaryXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParseSummary(xml);

        Assert.NotNull(parsed);
        Assert.Equal(original.Correlativo, parsed!.Correlativo);
        Assert.Single(parsed.Details);
    }

    [Fact]
    public void ParseVoided_RoundTrips()
    {
        var original = DocumentGenerator.CreateVoided();
        var xml = new VoidedXmlBuilder().Build(original);
        var parsed = XmlDocumentParser.ParseVoided(xml);

        Assert.NotNull(parsed);
        Assert.Equal(original.Correlativo, parsed!.Correlativo);
        Assert.Single(parsed.Details);
        Assert.Equal("01", parsed.Details[0].TipoDoc);
    }

    // ===== Edge cases =====

    [Fact]
    public void ParsePerception_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParsePerception(null!));
    }

    [Fact]
    public void ParseRetention_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParseRetention(null!));
    }

    [Fact]
    public void ParseSummary_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParseSummary(null!));
    }

    [Fact]
    public void ParseVoided_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParseVoided(null!));
    }

    [Fact]
    public void ParseInvoice_WithMinimalXml_ParsesBasicFields()
    {
        var xml = @"<?xml version=""1.0""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
    xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
    xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:ID>F001-1</cbc:ID>
    <cbc:IssueDate>2026-04-11</cbc:IssueDate>
</Invoice>";
        var parsed = XmlDocumentParser.ParseInvoice(xml);
        Assert.NotNull(parsed);
        Assert.Equal("F001", parsed!.Serie);
    }

    [Fact]
    public void ParseDespatch_WithoutShipment_StillParses()
    {
        var xml = @"<?xml version=""1.0""?>
<DespatchAdvice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2""
    xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
    xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:ID>T001-1</cbc:ID>
    <cbc:IssueDate>2026-04-11</cbc:IssueDate>
</DespatchAdvice>";
        var parsed = XmlDocumentParser.ParseDespatch(xml);
        Assert.NotNull(parsed);
        Assert.Equal("T001", parsed!.Serie);
        Assert.Empty(parsed.Details);
    }

    [Fact]
    public void ParseCreditNote_WithMultipleDetails_ParsesAll()
    {
        var note = DocumentGenerator.CreateCreditNote();
        note.Details.Add(new Khipu.Data.Documents.SaleDetail
        {
            Codigo = "P002", Descripcion = "Extra", Unidad = "NIU",
            Cantidad = 2, MtoValorUnitario = 100, MtoValorVenta = 200, PrecioVenta = 236
        });
        var xml = new CreditNoteXmlBuilder().Build(note);
        var parsed = XmlDocumentParser.ParseCreditNote(xml);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed!.Details.Count);
    }
}
