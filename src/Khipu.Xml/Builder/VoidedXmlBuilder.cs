namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Comunicación de Bajas
/// </summary>
public class VoidedXmlBuilder : IXmlBuilder<Voided>
{
    private static readonly XNamespace VoidedNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Voided voided)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateVoidedElement(voided)
        );
        
        return doc.ToString();
    }

    public string GetFileName(Voided voided)
    {
        var ruc = voided.Company.Ruc;
        var fecha = voided.FechaGeneracion.ToString("yyyyMMdd");
        var correlativo = voided.Correlativo.PadLeft(3, '0');
        return $"{ruc}-RA-{fecha}-{correlativo}.xml";
    }

    private XElement CreateVoidedElement(Voided voided)
    {
        return new XElement(VoidedNs + "VoidedDocuments",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            
            // UBL Extensions
            new XElement(ExtNs + "UBLExtensions",
                new XElement(ExtNs + "UBLExtension",
                    new XElement(ExtNs + "ExtensionContent")
                )
            ),
            
            // UBL Version
            new XElement(CbcNs + "UBLVersionID", "2.0"),
            new XElement(CbcNs + "CustomizationID", "1.1"),
            
            // ID de la comunicación
            new XElement(CbcNs + "ID", voided.Correlativo),
            
            // Fecha de referencia
            new XElement(CbcNs + "ReferenceDate", voided.FechaGeneracion.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueDate", voided.FechaEnvio.ToString("yyyy-MM-dd")),
            
            // Emisor
            new XElement(CacNs + "AccountingSupplierParty",
                new XElement(CacNs + "PartyAssignedAccountID",
                    new XElement(CbcNs + "AdditionalAccountID", "6"),
                    voided.Company.Ruc
                )
            ),
            
            // Documentos a dar de baja
            voided.Details.Select((d, i) => CreateVoidedDocumentLine(d, i + 1))
        );
    }

    private XElement CreateVoidedDocumentLine(VoidedDetail detail, int lineNumber)
    {
        return new XElement(CacNs + "VoidedDocumentsLine",
            new XElement(CbcNs + "LineID", lineNumber),
            new XElement(CbcNs + "DocumentTypeCode", detail.TipoDoc),
            new XElement(CbcNs + "DocumentSerialID", detail.SerieNro.Split('-')[0]),
            new XElement(CbcNs + "DocumentNumberID", detail.SerieNro.Split('-')[1]),
            new XElement(CbcNs + "VoidReasonDescription", new XCData(detail.MotivoBaja))
        );
    }
}
