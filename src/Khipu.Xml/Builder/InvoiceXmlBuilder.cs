namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Facturas
/// </summary>
public class InvoiceXmlBuilder : XmlBuilderBase, IXmlBuilder<Invoice>
{
    private static readonly XNamespace InvoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    public string Build(Invoice invoice)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateInvoiceElement(invoice)
        );
        
        return doc.ToString();
    }

    public string GetFileName(Invoice invoice)
    {
        var ruc = invoice.Company.Ruc;
        var tipoDoc = ((int)invoice.TipoDoc).ToString().PadLeft(2, '0');
        var serie = invoice.Serie;
        var correlativo = invoice.Correlativo.ToString().PadLeft(8, '0');
        return $"{ruc}-{tipoDoc}-{serie}-{correlativo}.xml";
    }

    private XElement CreateInvoiceElement(Invoice invoice)
    {
        return new XElement(InvoiceNs + "Invoice",
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
            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "2.0"),
            new XElement(CbcNs + "ID", $"{invoice.Serie}-{invoice.Correlativo}"),
            new XElement(CbcNs + "IssueDate", invoice.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", invoice.FechaEmision.ToString("HH:mm:ss")),
            new XElement(CbcNs + "InvoiceTypeCode", 
                new XAttribute("listID", invoice.TipoOperacion ?? "0101"),
                ((int)invoice.TipoDoc).ToString().PadLeft(2, '0')
            ),
            new XElement(CbcNs + "DocumentCurrencyCode", GetCurrencyCode(invoice.Moneda)),
            
            // Firma, Emisor, Receptor, Totales, Detalles
            CreateSignature(invoice.Company),
            CreateSupplierParty(invoice.Company),
            CreateCustomerParty(invoice.Client),
            CreateLegalMonetaryTotal(invoice),
            CreateTaxTotal(invoice),
            invoice.Details.Select((d, i) => CreateInvoiceLine(d, i + 1, invoice.Moneda))
        );
    }
}
