namespace Khipu.Tests;

using Khipu.Data.Generators;
using Khipu.Xml.Builder;
using Khipu.Xml.Parser;
using Xunit;

public class XmlParserExtendedTests
{
    [Fact]
    public void ParsePerception_RoundTrips()
    {
        var original = DocumentGenerator.CreatePerception();
        var xml = new PerceptionXmlBuilder().Build(original);

        var parsed = XmlDocumentParser.ParsePerception(xml);

        Assert.NotNull(parsed);
        Assert.Equal("P001", parsed!.Serie);
        Assert.Equal(1, parsed.Correlativo);
        Assert.Equal(original.MtoPercepcion, parsed.MtoPercepcion);
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
        Assert.Equal(1, parsed.Correlativo);
        Assert.Equal(original.MtoRetencion, parsed.MtoRetencion);
        Assert.Single(parsed.Details);
        Assert.Equal("F001-00000001", parsed.Details[0].NumDoc);
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
        Assert.Equal("B001-00000001", parsed.Details[0].SerieNro);
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
        Assert.Contains("F001", parsed.Details[0].SerieNro);
    }

    [Fact]
    public void ParsePerception_WithNull_ReturnsNull()
    {
        Assert.Null(XmlDocumentParser.ParsePerception(null!));
        Assert.Null(XmlDocumentParser.ParsePerception(""));
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
    public void DocumentGenerator_CreatesAllTypes()
    {
        Assert.NotNull(DocumentGenerator.CreateInvoice());
        Assert.NotNull(DocumentGenerator.CreateReceipt());
        Assert.NotNull(DocumentGenerator.CreateCreditNote());
        Assert.NotNull(DocumentGenerator.CreateDebitNote());
        Assert.NotNull(DocumentGenerator.CreateDespatch());
        Assert.NotNull(DocumentGenerator.CreatePerception());
        Assert.NotNull(DocumentGenerator.CreateRetention());
        Assert.NotNull(DocumentGenerator.CreateSummary());
        Assert.NotNull(DocumentGenerator.CreateVoided());
    }

    [Fact]
    public void DocumentGenerator_InvoiceHasValidData()
    {
        var invoice = DocumentGenerator.CreateInvoice();

        Assert.Equal("20100070970", invoice.Company.Ruc);
        Assert.Equal("F001", invoice.Serie);
        Assert.Equal(2, invoice.Details.Count);
        Assert.Equal(1180.00m, invoice.MtoImpVenta);
        Assert.Equal(180.00m, invoice.MtoIGV);
    }

    [Fact]
    public void FullPipeline_Invoice_BuildParseRoundTrip()
    {
        var original = DocumentGenerator.CreateInvoice();
        var builder = new InvoiceXmlBuilder();

        // Build XML
        var xml = builder.Build(original);
        Assert.Contains("Invoice", xml);

        // Parse back
        var parsed = XmlDocumentParser.ParseInvoice(xml);
        Assert.NotNull(parsed);
        Assert.Equal(original.Serie, parsed!.Serie);
        Assert.Equal(original.Correlativo, parsed.Correlativo);
        Assert.Equal(original.Company.Ruc, parsed.Company.Ruc);
        Assert.Equal(original.Details.Count, parsed.Details.Count);
    }

    [Fact]
    public void FullPipeline_Despatch_BuildParseRoundTrip()
    {
        var original = DocumentGenerator.CreateDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(original);
        Assert.Contains("DespatchAdvice", xml);

        var parsed = XmlDocumentParser.ParseDespatch(xml);
        Assert.NotNull(parsed);
        Assert.Equal(original.Serie, parsed!.Serie);
        Assert.Equal(original.CodMotivoTraslado, parsed.CodMotivoTraslado);
        Assert.Equal(original.Details.Count, parsed.Details.Count);
    }
}
