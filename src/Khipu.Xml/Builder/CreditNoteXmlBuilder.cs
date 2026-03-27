namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Enums;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Notas de Crédito - Paridad 100% Greenter
/// </summary>
public class CreditNoteXmlBuilder : XmlBuilderBase, IXmlBuilder<CreditNote>
{
    private static readonly XNamespace CreditNoteNs = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";

    public string Build(CreditNote note)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateCreditNoteElement(note)
        );

        return doc.ToString();
    }

    public string GetFileName(CreditNote note)
    {
        var ruc = note.Company.Ruc;
        var tipoDoc = ((int)note.TipoDoc).ToString().PadLeft(2, '0');
        var serie = note.Serie;
        var correlativo = note.Correlativo.ToString().PadLeft(8, '0');
        return $"{ruc}-{tipoDoc}-{serie}-{correlativo}.xml";
    }

    private XElement CreateCreditNoteElement(CreditNote note)
    {
        return new XElement(CreditNoteNs + "CreditNote",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),

            new XElement(ExtNs + "UBLExtensions",
                new XElement(ExtNs + "UBLExtension",
                    new XElement(ExtNs + "ExtensionContent")
                )
            ),

            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "2.0"),
            new XElement(CbcNs + "ID", $"{note.Serie}-{note.Correlativo}"),
            new XElement(CbcNs + "IssueDate", note.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", note.FechaEmision.ToString("HH:mm:ss")),
            note.Leyendas?.Count > 0 ? CreateNoteLegends(note) : null,
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

            CreateSignature(note.Company),
            CreateSupplierParty(note.Company),
            CreateCustomerParty(note.Client),
            CreateTaxTotal(note),
            CreateLegalMonetaryTotal(note),
            note.Details.Select((d, i) => CreateCreditNoteLine(d, i + 1, note.Moneda))
        );
    }

    private object? CreateNoteLegends(CreditNote note)
    {
        return note.Leyendas!
            .Where(l => !string.IsNullOrWhiteSpace(l.Code) && !string.IsNullOrWhiteSpace(l.Value))
            .Select(l => new XElement(CbcNs + "Note",
                new XAttribute("languageLocaleID", l.Code),
                new XCData(l.Value)))
            .ToArray();
    }

    private XElement CreateCreditNoteLine(SaleDetail detail, int lineNumber, Currency currency)
    {
        var currencyCode = GetCurrencyCode(currency);
        var tributo = Khipu.Data.Algorithms.TributoFunction.GetByAfectacion(detail.TipoAfectacionIgv);
        var taxRate = detail.TasaIgv <= 0 ? 0.18m : detail.TasaIgv;
        var igvAmount = detail.Igv ?? (detail.MtoValorVenta * taxRate);
        var baseIgv = detail.MtoBaseIgv ?? detail.MtoValorVenta;
        var afectCode = ((int)detail.TipoAfectacionIgv).ToString();

        var lineTaxes = new List<XElement>();
        var totalLineTax = igvAmount;

        // ISC
        if (detail.MtoIsc.HasValue && detail.MtoIsc > 0)
        {
            totalLineTax += detail.MtoIsc.Value;
        }

        // OtroTributo
        if (detail.OtroTributo.HasValue && detail.OtroTributo > 0)
        {
            totalLineTax += detail.OtroTributo.Value;
        }

        // ICBPER
        if (detail.Icbper.HasValue && detail.Icbper > 0)
        {
            totalLineTax += detail.Icbper.Value;
        }

        return new XElement(CacNs + "CreditNoteLine",
            new XElement(CbcNs + "ID", lineNumber),
            new XElement(CbcNs + "CreditedQuantity",
                new XAttribute("unitCode", detail.Unidad),
                detail.Cantidad.ToString("F2")
            ),
            new XElement(CbcNs + "LineExtensionAmount",
                new XAttribute("currencyID", currencyCode),
                FormatAmount(detail.MtoValorVenta)
            ),
            new XElement(CacNs + "PricingReference",
                new XElement(CacNs + "AlternativeConditionPrice",
                    new XElement(CbcNs + "PriceAmount",
                        new XAttribute("currencyID", currencyCode),
                        FormatAmount(detail.PrecioVenta)
                    ),
                    new XElement(CbcNs + "PriceTypeCode", "01")
                )
            ),
            CreateLineCharges(detail.Descuentos, false, currencyCode),
            CreateLineCharges(detail.Cargos, true, currencyCode),
            new XElement(CacNs + "TaxTotal",
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(totalLineTax)
                ),
                detail.MtoIsc.HasValue && detail.MtoIsc > 0 ?
                    new XElement(CacNs + "TaxSubtotal",
                        new XElement(CbcNs + "TaxableAmount", new XAttribute("currencyID", currencyCode), FormatAmount(detail.MtoBaseIsc ?? detail.MtoValorVenta)),
                        new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", currencyCode), FormatAmount(detail.MtoIsc!.Value)),
                        new XElement(CacNs + "TaxCategory",
                            new XElement(CbcNs + "TaxScheme",
                                new XElement(CbcNs + "ID", "2000"),
                                new XElement(CbcNs + "Name", "ISC"),
                                new XElement(CbcNs + "TaxTypeCode", "EXC")))) : null,
                new XElement(CacNs + "TaxSubtotal",
                    new XElement(CbcNs + "TaxableAmount",
                        new XAttribute("currencyID", currencyCode),
                        FormatAmount(baseIgv)),
                    new XElement(CbcNs + "TaxAmount",
                        new XAttribute("currencyID", currencyCode),
                        FormatAmount(igvAmount)
                    ),
                    new XElement(CacNs + "TaxCategory",
                        new XElement(CbcNs + "Percent", FormatAmount(detail.PorcentajeIgv ?? (taxRate * 100m))),
                        new XElement(CbcNs + "TaxExemptionReasonCode", afectCode),
                        new XElement(CbcNs + "TaxScheme",
                            new XElement(CbcNs + "ID", tributo?.Id ?? "1000"),
                            new XElement(CbcNs + "Name", tributo?.Name ?? "IGV"),
                            new XElement(CbcNs + "TaxTypeCode", tributo?.Code ?? "VAT")
                        )
                    )
                ),
                detail.Icbper.HasValue && detail.Icbper > 0 ?
                    new XElement(CacNs + "TaxSubtotal",
                        new XElement(CbcNs + "TaxableAmount", new XAttribute("currencyID", currencyCode), FormatAmount(0m)),
                        new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", currencyCode), FormatAmount(detail.Icbper!.Value)),
                        new XElement(CacNs + "TaxCategory",
                            new XElement(CbcNs + "TaxScheme",
                                new XElement(CbcNs + "ID", "7152"),
                                new XElement(CbcNs + "Name", "ICBPER"),
                                new XElement(CbcNs + "TaxTypeCode", "OTH")))) : null
            ),
            new XElement(CacNs + "Item",
                new XElement(CbcNs + "Description", new XCData(detail.Descripcion)),
                new XElement(CacNs + "SellersItemIdentification",
                    new XElement(CbcNs + "ID", detail.Codigo)
                ),
                !string.IsNullOrEmpty(detail.CodProdSunat) ?
                    new XElement(CacNs + "CommodityClassification",
                        new XElement(CbcNs + "ItemClassificationCode",
                            new XAttribute("listID", "UNSPSC"), detail.CodProdSunat)) : null
            ),
            new XElement(CacNs + "Price",
                new XElement(CbcNs + "PriceAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(detail.MtoValorUnitario)
                )
            )
        );
    }
}
