namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Enums;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Resumen de Boletas
/// </summary>
public class SummaryXmlBuilder : IXmlBuilder<Summary>
{
    private static readonly XNamespace SummaryNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Summary summary)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateSummaryElement(summary)
        );
        
        return doc.ToString();
    }

    public string GetFileName(Summary summary)
    {
        var ruc = summary.Company.Ruc;
        var fecha = summary.FechaGeneracion.ToString("yyyyMMdd");
        var correlativo = summary.Correlativo.PadLeft(3, '0');
        return $"{ruc}-RC-{fecha}-{correlativo}.xml";
    }

    private XElement CreateSummaryElement(Summary summary)
    {
        return new XElement(SummaryNs + "SummaryDocuments",
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
            
            // ID del resumen
            new XElement(CbcNs + "ID", summary.Correlativo),
            
            // Referencia fiscal
            new XElement(CacNs + "AccountingSupplierParty",
                new XElement(CacNs + "PartyAssignedAccountID",
                    summary.Company.Ruc
                )
            ),
            
            // Fecha de emisión
            new XElement(CbcNs + "IssueDate", summary.FechaGeneracion.ToString("yyyy-MM-dd")),
            
            // Detalles
            summary.Details.Select((d, i) => CreateSummaryDocument(d, i + 1))
        );
    }

    private XElement CreateSummaryDocument(SummaryDetail detail, int lineNumber)
    {
        return new XElement(CacNs + "SummaryDocumentsLine",
            new XElement(CbcNs + "LineID", lineNumber),
            new XElement(CbcNs + "DocumentTypeCode", ((int)detail.TipoDoc).ToString().PadLeft(2, '0')),
            new XElement(CbcNs + "ID", detail.SerieNro),
            new XElement(CbcNs + "AccountingCustomerPartyID", detail.ClienteNroDoc),
            
            // Totales
            new XElement(CacNs + "BillingReference",
                new XElement(CacNs + "InvoiceDocumentReference",
                    new XElement(CbcNs + "TotalInvoiceAmount",
                        detail.MtoImpVenta.ToString("F2")
                    )
                )
            ),
            
            // Impuestos
            new XElement(CacNs + "TaxTotal",
                new XElement(CbcNs + "TaxAmount", detail.MtoIGV.ToString("F2"))
            )
        );
    }
}
