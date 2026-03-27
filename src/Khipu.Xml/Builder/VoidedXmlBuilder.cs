namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

public class VoidedXmlBuilder : IXmlBuilder<Voided>
{
    private static readonly XNamespace VoidedNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1";
    private static readonly XNamespace SacNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Voided voided)
    {
        EnsureNoDuplicateReferences(voided);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreateVoidedElement(voided));
        return doc.ToString();
    }

    public string GetFileName(Voided voided)
    {
        var fecha = voided.FechaGeneracion.ToString("yyyyMMdd");
        var correlativo = voided.Correlativo.PadLeft(3, '0');
        return $"{voided.Company.Ruc}-RA-{fecha}-{correlativo}.xml";
    }

    private XElement CreateVoidedElement(Voided voided)
    {
        return new XElement(VoidedNs + "VoidedDocuments",
            new XAttribute(XNamespace.Xmlns + "sac", SacNs),
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            new XElement(ExtNs + "UBLExtensions", new XElement(ExtNs + "UBLExtension", new XElement(ExtNs + "ExtensionContent"))),
            new XElement(CbcNs + "UBLVersionID", "2.0"),
            new XElement(CbcNs + "CustomizationID", "1.0"),
            new XElement(CbcNs + "ID", voided.Correlativo.PadLeft(3, '0')),
            new XElement(CbcNs + "ReferenceDate", voided.FechaGeneracion.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueDate", voided.FechaEnvio.ToString("yyyy-MM-dd")),
            new XElement(CacNs + "AccountingSupplierParty",
                new XElement(CbcNs + "CustomerAssignedAccountID", voided.Company.Ruc),
                new XElement(CbcNs + "AdditionalAccountID", "6"),
                new XElement(CacNs + "Party", new XElement(CacNs + "PartyLegalEntity", new XElement(CbcNs + "RegistrationName", voided.Company.RazonSocial)))),
            voided.Details.OrderBy(d => d.Orden == 0 ? int.MaxValue : d.Orden).Select((d, i) => CreateVoidedDocumentLine(d, i + 1))
        );
    }

    private XElement CreateVoidedDocumentLine(VoidedDetail detail, int lineNumber)
    {
        var (serial, number) = ParseReference(detail.SerieNro);

        return new XElement(SacNs + "VoidedDocumentsLine",
            new XElement(CbcNs + "LineID", lineNumber),
            new XElement(CbcNs + "DocumentTypeCode", detail.TipoDoc),
            new XElement(SacNs + "DocumentSerialID", serial),
            new XElement(SacNs + "DocumentNumberID", number),
            new XElement(SacNs + "VoidReasonDescription", new XCData(detail.MotivoBaja))
        );
    }

    private static void EnsureNoDuplicateReferences(Voided voided)
    {
        var duplicated = voided.Details
            .GroupBy(d => d.SerieNro, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicated is not null)
        {
            throw new InvalidOperationException($"DUPLICATE_REFERENCE:{duplicated.Key}");
        }
    }

    private static (string Serial, string Number) ParseReference(string serieNro)
    {
        if (string.IsNullOrWhiteSpace(serieNro))
        {
            throw new InvalidOperationException("INVALID_REFERENCE:");
        }

        var parts = serieNro.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"INVALID_REFERENCE:{serieNro}");
        }

        var serial = parts[0];
        var number = parts[1];
        if (number.All(char.IsDigit))
        {
            number = int.TryParse(number, out var parsed) ? parsed.ToString("000") : number;
        }

        return (serial, number);
    }
}
