namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Notas de Débito
/// </summary>
public class DebitNoteXmlBuilder : XmlBuilderBase, IXmlBuilder<DebitNote>
{
    private static readonly XNamespace DebitNoteNs = "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2";

    public string Build(DebitNote note)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateDebitNoteElement(note)
        );
        
        return doc.ToString();
    }

    public string GetFileName(DebitNote note)
    {
        var ruc = note.Company.Ruc;
        var tipoDoc = ((int)note.TipoDoc).ToString().PadLeft(2, '0');
        var serie = note.Serie;
        var correlativo = note.Correlativo.ToString().PadLeft(8, '0');
        return $"{ruc}-{tipoDoc}-{serie}-{correlativo}.xml";
    }

    private XElement CreateDebitNoteElement(DebitNote note)
    {
        return new XElement(DebitNoteNs + "DebitNote",
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
            new XElement(CbcNs + "ID", $"{note.Serie}-{note.Correlativo}"),
            new XElement(CbcNs + "IssueDate", note.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", note.FechaEmision.ToString("HH:mm:ss")),
            new XElement(CbcNs + "DocumentCurrencyCode", GetCurrencyCode(note.Moneda)),
            
            // DiscrepancyResponse (motivo de la nota)
            new XElement(CacNs + "DiscrepancyResponse",
                new XElement(CbcNs + "ReferenceID", note.NumDocAfectado),
                new XElement(CbcNs + "ResponseCode", note.CodMotivo),
                new XElement(CbcNs + "Description", new XCData(note.DesMotivo))
            ),
            
            // Documento afectado
            new XElement(CacNs + "BillingReference",
                new XElement(CacNs + "InvoiceDocumentReference",
                    new XElement(CbcNs + "ID", note.NumDocAfectado),
                    new XElement(CbcNs + "DocumentTypeCode", note.TipDocAfectado)
                )
            ),
            
            // Firma, Emisor, Receptor, Totales, Detalles
            CreateSignature(note.Company),
            CreateSupplierParty(note.Company),
            CreateCustomerParty(note.Client),
            CreateLegalMonetaryTotal(note),
            CreateTaxTotal(note),
            note.Details.Select((d, i) => CreateDebitNoteLine(d, i + 1, note.Moneda))
        );
    }

    private XElement CreateDebitNoteLine(SaleDetail detail, int lineNumber, Data.Enums.Currency currency)
    {
        return new XElement(CacNs + "DebitNoteLine",
            new XElement(CbcNs + "ID", lineNumber),
            new XElement(CbcNs + "DebitedQuantity",
                new XAttribute("unitCode", detail.Unidad),
                detail.Cantidad.ToString("F2")
            ),
            new XElement(CbcNs + "LineExtensionAmount",
                detail.MtoValorVenta.ToString("F2")
            ),
            new XElement(CacNs + "PricingReference",
                new XElement(CacNs + "AlternativeConditionPrice",
                    new XElement(CbcNs + "PriceAmount",
                        new XAttribute("currencyID", GetCurrencyCode(currency)),
                        detail.PrecioVenta.ToString("F2")
                    ),
                    new XElement(CbcNs + "PriceTypeCode", "01")
                )
            ),
            new XElement(CacNs + "TaxTotal",
                new XElement(CbcNs + "TaxAmount",
                    (detail.MtoValorVenta * 0.18m).ToString("F2")
                ),
                new XElement(CacNs + "TaxSubtotal",
                    new XElement(CbcNs + "TaxAmount",
                        (detail.MtoValorVenta * 0.18m).ToString("F2")
                    ),
                    new XElement(CacNs + "TaxCategory",
                        new XElement(CbcNs + "TaxScheme",
                            new XElement(CbcNs + "ID", "1000"),
                            new XElement(CbcNs + "Name", "IGV"),
                            new XElement(CbcNs + "TaxTypeCode", "VAT")
                        )
                    )
                )
            ),
            new XElement(CacNs + "Item",
                new XElement(CacNs + "SellersItemIdentification",
                    new XElement(CbcNs + "ID", detail.Codigo)
                ),
                new XElement(CbcNs + "Description", new XCData(detail.Descripcion))
            ),
            new XElement(CacNs + "Price",
                new XElement(CbcNs + "PriceAmount",
                    detail.MtoValorUnitario.ToString("F2")
                )
            )
        );
    }
}
