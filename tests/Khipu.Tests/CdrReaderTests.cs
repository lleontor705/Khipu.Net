namespace Khipu.Tests;

using Khipu.Ws.Reader;
using Xunit;

public class CdrReaderTests
{
    [Fact]
    public void Parse_WithAcceptedCdr_ReturnsAccepted()
    {
        var xml = CreateCdrXml("0", "La Factura numero F001-00000001, ha sido aceptada", "F001-00000001");

        var result = CdrReader.Parse(xml);

        Assert.NotNull(result);
        Assert.Equal("0", result!.Code);
        Assert.True(result.IsAccepted);
        Assert.Contains("aceptada", result.Description!);
        Assert.Equal("F001-00000001", result.Id);
    }

    [Fact]
    public void Parse_WithRejectedCdr_ReturnsNotAccepted()
    {
        var xml = CreateCdrXml("2017", "El monto total del comprobante difiere del cálculo", "F001-00000002");

        var result = CdrReader.Parse(xml);

        Assert.NotNull(result);
        Assert.Equal("2017", result!.Code);
        Assert.False(result.IsAccepted);
    }

    [Fact]
    public void Parse_WithObservation4000_ReturnsAccepted()
    {
        var xml = CreateCdrXml("4000", "Observaciones menores", "F001-00000003");

        var result = CdrReader.Parse(xml);

        Assert.NotNull(result);
        Assert.Equal("4000", result!.Code);
        Assert.True(result.IsAccepted);
    }

    [Fact]
    public void Parse_WithNotes_ExtractsNotes()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ApplicationResponse xmlns=""urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2""
    xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
    xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:Note>Nota adicional 1</cbc:Note>
    <cbc:Note>Nota adicional 2</cbc:Note>
    <cac:DocumentResponse>
        <cac:Response>
            <cbc:ReferenceID>F001-1</cbc:ReferenceID>
            <cbc:ResponseCode>0</cbc:ResponseCode>
            <cbc:Description>Aceptado</cbc:Description>
        </cac:Response>
    </cac:DocumentResponse>
</ApplicationResponse>";

        var result = CdrReader.Parse(xml);

        Assert.NotNull(result);
        Assert.NotNull(result!.Notes);
        Assert.Equal(2, result.Notes!.Count);
        Assert.Equal("Nota adicional 1", result.Notes[0]);
        Assert.Equal("Nota adicional 2", result.Notes[1]);
    }

    [Fact]
    public void Parse_WithDocumentReference_ExtractsReference()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ApplicationResponse xmlns=""urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2""
    xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
    xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cac:DocumentResponse>
        <cac:Response>
            <cbc:ReferenceID>F001-1</cbc:ReferenceID>
            <cbc:ResponseCode>0</cbc:ResponseCode>
            <cbc:Description>Aceptado</cbc:Description>
        </cac:Response>
        <cac:DocumentReference>
            <cbc:DocumentDescription>20123456789-01-F001-00000001</cbc:DocumentDescription>
        </cac:DocumentReference>
    </cac:DocumentResponse>
</ApplicationResponse>";

        var result = CdrReader.Parse(xml);

        Assert.NotNull(result);
        Assert.Equal("20123456789-01-F001-00000001", result!.Reference);
    }

    [Fact]
    public void Parse_WithNullXml_ReturnsNull()
    {
        Assert.Null(CdrReader.Parse(null));
        Assert.Null(CdrReader.Parse(""));
        Assert.Null(CdrReader.Parse("   "));
    }

    [Fact]
    public void Parse_WithInvalidXml_ReturnsNull()
    {
        Assert.Null(CdrReader.Parse("<invalid>xml"));
    }

    [Fact]
    public void ParseFromZip_WithNullData_ReturnsNull()
    {
        Assert.Null(CdrReader.ParseFromZip(null));
        Assert.Null(CdrReader.ParseFromZip(Array.Empty<byte>()));
    }

    [Fact]
    public void ParseFromZip_WithValidZip_ParsesCdr()
    {
        var xml = CreateCdrXml("0", "Aceptado", "F001-1");
        var zip = Khipu.Ws.Helpers.ZipHelper.CreateZip("R-20123456789-01-F001-00000001", xml);

        var result = CdrReader.ParseFromZip(zip);

        Assert.NotNull(result);
        Assert.Equal("0", result!.Code);
        Assert.True(result.IsAccepted);
    }

    private static string CreateCdrXml(string code, string description, string referenceId)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ApplicationResponse xmlns=""urn:oasis:names:specification:ubl:schema:xsd:ApplicationResponse-2""
    xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
    xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cac:DocumentResponse>
        <cac:Response>
            <cbc:ReferenceID>{referenceId}</cbc:ReferenceID>
            <cbc:ResponseCode>{code}</cbc:ResponseCode>
            <cbc:Description>{description}</cbc:Description>
        </cac:Response>
    </cac:DocumentResponse>
</ApplicationResponse>";
    }
}
