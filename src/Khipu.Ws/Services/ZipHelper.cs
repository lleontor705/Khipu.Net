namespace Khipu.Ws.Helpers;
using System.IO.Compression;
using System.Xml.Linq;

public static class ZipHelper
{
    public static byte[] CreateZip(string fileName, string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("fileName is required", nameof(fileName));
        }

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var normalized = fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : $"{fileName}.xml";
            var entry = archive.CreateEntry(normalized);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(xmlContent);
        }

        return ms.ToArray();
    }

    public static string ExtractXml(byte[] zipData)
    {
        if (zipData is null || zipData.Length == 0)
        {
            throw new InvalidDataException("ZIP content is empty");
        }

        using var ms = new MemoryStream(zipData);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

        var xmlEntries = archive.Entries
            .Where(entry => entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (xmlEntries.Count != 1)
        {
            throw new InvalidDataException("ZIP must contain exactly one XML entry");
        }

        var entry = xmlEntries[0];
        using var reader = new StreamReader(entry.Open());
        var xml = reader.ReadToEnd();

        // Validate XML well-formedness
        try { XDocument.Parse(xml); }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidDataException($"ZIP contains invalid XML: {ex.Message}", ex);
        }

        return xml;
    }
}
