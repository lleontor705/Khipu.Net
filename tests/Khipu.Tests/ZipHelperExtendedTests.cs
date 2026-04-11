namespace Khipu.Tests;

using Khipu.Ws.Helpers;
using Xunit;

public class ZipHelperExtendedTests
{
    [Fact]
    public void CreateZip_WithXmlExtension_DoesNotDoubleExtension()
    {
        var zip = ZipHelper.CreateZip("test.xml", "<root/>");
        var xml = ZipHelper.ExtractXml(zip);
        Assert.Equal("<root/>", xml.Trim());
    }

    [Fact]
    public void CreateZip_WithoutExtension_AddsXml()
    {
        var zip = ZipHelper.CreateZip("test", "<root/>");
        var xml = ZipHelper.ExtractXml(zip);
        Assert.Equal("<root/>", xml.Trim());
    }

    [Fact]
    public void CreateZip_WithEmptyFileName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ZipHelper.CreateZip("", "<root/>"));
    }

    [Fact]
    public void ExtractXml_WithEmptyBytes_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => ZipHelper.ExtractXml(Array.Empty<byte>()));
    }

    [Fact]
    public void ExtractXml_WithNullBytes_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => ZipHelper.ExtractXml(null!));
    }

    [Fact]
    public void ExtractXml_WithInvalidXml_ThrowsInvalidDataException()
    {
        var zip = CreateZipWithContent("test.xml", "not valid xml <><>");
        Assert.Throws<InvalidDataException>(() => ZipHelper.ExtractXml(zip));
    }

    [Fact]
    public void CreateAndExtract_RoundTrips()
    {
        var original = "<?xml version=\"1.0\"?><Invoice><ID>F001-1</ID></Invoice>";
        var zip = ZipHelper.CreateZip("20100070970-01-F001-00000001", original);
        var extracted = ZipHelper.ExtractXml(zip);
        Assert.Equal(original, extracted);
    }

    private static byte[] CreateZipWithContent(string fileName, string content)
    {
        using var ms = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(fileName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }
        return ms.ToArray();
    }
}
