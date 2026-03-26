namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Boletas de Venta
/// </summary>
public class ReceiptXmlBuilder : XmlBuilderBase, IXmlBuilder<Receipt>
{
    private static readonly XNamespace InvoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    public string Build(Receipt receipt)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateReceiptElement(receipt)
        );
        
        return doc.ToString();
    }

    public string GetFileName(Receipt receipt)
    {
        var ruc = receipt.Company.Ruc;
        var tipoDoc = ((int)receipt.TipoDoc).ToString().PadLeft(2, '0');
        var serie = receipt.Serie;
        var correlativo = receipt.Correlativo.ToString().PadLeft(8, '0');
        return $"{ruc}-{tipoDoc}-{serie}-{correlativo}.xml";
    }

    private XElement CreateReceiptElement(Receipt receipt)
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
            new XElement(CbcNs + "ID", $"{receipt.Serie}-{receipt.Correlativo}"),
            new XElement(CbcNs + "IssueDate", receipt.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", receipt.FechaEmision.ToString("HH:mm:ss")),
            new XElement(CbcNs + "InvoiceTypeCode", 
                new XAttribute("listID", "0101"),
                ((int)receipt.TipoDoc).ToString().PadLeft(2, '0')
            ),
            new XElement(CbcNs + "DocumentCurrencyCode", GetCurrencyCode(receipt.Moneda)),
            
            // Firma, Emisor, Receptor, Totales, Detalles
            CreateSignature(receipt.Company),
            CreateSupplierParty(receipt.Company),
            CreateCustomerParty(receipt.Client),
            CreateLegalMonetaryTotal(receipt),
            CreateTaxTotal(receipt),
            receipt.Details.Select((d, i) => CreateInvoiceLine(d, i + 1, receipt.Moneda))
        );
    }
}
