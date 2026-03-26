namespace Khipu.Ws.Helpers;
using System.IO.Compression;
public static class ZipHelper
{
    public static byte[] CreateZip(string fileName, string xmlContent)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry(fileName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(xmlContent);
        }
        return ms.ToArray();
    }
    public static string ExtractXml(byte[] zipData)
    {
        using var ms = new MemoryStream(zipData);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        var entry = archive.Entries[0];
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }
}
