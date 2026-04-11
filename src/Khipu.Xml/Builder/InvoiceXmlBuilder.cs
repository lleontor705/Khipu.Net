namespace Khipu.Xml.Builder;

using System.Collections.Generic;
using System.Xml.Linq;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Facturas - Paridad 100% Greenter
/// </summary>
public class InvoiceXmlBuilder : XmlBuilderBase, IXmlBuilder<Invoice>
{
    private static readonly XNamespace InvoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    public string Build(Invoice invoice)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreateInvoiceElement(invoice));
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
        var currency = GetCurrencyCode(invoice.Moneda);

        return new XElement(InvoiceNs + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            new XElement(ExtNs + "UBLExtensions", new XElement(ExtNs + "UBLExtension", new XElement(ExtNs + "ExtensionContent"))),
            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "2.0"),
            new XElement(CbcNs + "ID", $"{invoice.Serie}-{invoice.Correlativo}"),
            new XElement(CbcNs + "IssueDate", invoice.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", invoice.FechaEmision.ToString("HH:mm:ss")),
            invoice.FecVencimiento.HasValue ? new XElement(CbcNs + "DueDate", invoice.FecVencimiento.Value.ToString("yyyy-MM-dd")) : null,
            new XElement(CbcNs + "InvoiceTypeCode", new XAttribute("listID", invoice.TipoOperacion ?? "0101"), ((int)invoice.TipoDoc).ToString().PadLeft(2, '0')),
            invoice.Leyendas?.Count > 0 ? CreateLegends(invoice).ToArray() : null,
            // Observación como Note sin languageLocaleID (Greenter: doc.observacion)
            !string.IsNullOrEmpty(invoice.Observacion) ?
                new XElement(CbcNs + "Note", new XCData(invoice.Observacion)) : null,
            new XElement(CbcNs + "DocumentCurrencyCode", currency),
            // Orden de compra (Greenter: compra)
            !string.IsNullOrEmpty(invoice.Compra) ?
                new XElement(CacNs + "OrderReference",
                    new XElement(CbcNs + "ID", invoice.Compra)) : null,
            // Guías de remisión (Greenter: guias)
            CreateRelatedDocuments(invoice.Guias),
            // Documentos relacionados (Greenter: relDocs)
            CreateAdditionalDocuments(invoice.RelDocs),
            CreateSignature(invoice.Company),
            CreateSupplierParty(invoice.Company),
            CreateCustomerParty(invoice.Client),
            // Seller diferente (Greenter: seller)
            invoice.Seller != null ? CreateSellerSupplierParty(invoice.Seller) : null,
            // Delivery (Greenter: dirección de entrega)
            invoice.DireccionEntrega != null ? CreateDelivery(invoice.DireccionEntrega) : null,
            // Detracción
            invoice.Detraccion?.Monto > 0 ? CreateDetraction(invoice).ToArray() : null,
            // Forma de pago (Greenter: formaPago + cuotas)
            CreatePaymentTerms(invoice, currency),
            // Anticipos
            invoice.Anticipos?.Count > 0 ? CreatePrepayments(invoice).ToArray() : null,
            // Cargos y descuentos globales (Greenter: cargos/descuentos)
            CreateGlobalCharges(invoice.Cargos, true, currency),
            CreateGlobalCharges(invoice.Descuentos, false, currency),
            // Perception AllowanceCharge (Greenter: doc.perception → AllowanceCharge)
            invoice.Perception != null ? CreatePerceptionAllowanceCharge(invoice) : null,
            // Perception PaymentTerms (Greenter: if doc.perception → PaymentTerms ID=Percepcion)
            invoice.Perception != null ? CreatePerceptionPaymentTerms(invoice) : null,
            // Tax totals (multi-impuesto)
            CreateTaxTotals(invoice),
            CreateInvoiceLegalMonetaryTotal(invoice),
            invoice.Details
                .OrderBy(d => d.Orden == 0 ? int.MaxValue : d.Orden)
                .Select((d, i) => CreateInvoiceLine(d, i + 1, invoice.Moneda))
        );
    }

    private IEnumerable<XElement> CreateLegends(Invoice invoice)
    {
        return invoice.Leyendas!
            .Where(legend => !string.IsNullOrWhiteSpace(legend.Code) && !string.IsNullOrWhiteSpace(legend.Value))
            .Select(legend =>
                new XElement(CbcNs + "Note",
                    new XAttribute("languageLocaleID", legend.Code),
                    new XCData(legend.Value)));
    }

    private IEnumerable<XElement> CreateDetraction(Invoice invoice)
    {
        var currency = GetCurrencyCode(invoice.Moneda);

        return new XElement[]
        {
            new XElement(CacNs + "PaymentMeans",
                new XElement(CbcNs + "ID", "Detraccion"),
                new XElement(CbcNs + "PaymentMeansCode", invoice.Detraccion!.CodMedioPago ?? "001"),
                !string.IsNullOrWhiteSpace(invoice.Detraccion.CtaBanco)
                    ? new XElement(CacNs + "PayeeFinancialAccount",
                        new XElement(CbcNs + "ID", invoice.Detraccion.CtaBanco))
                    : null),
            new XElement(CacNs + "PaymentTerms",
                new XElement(CbcNs + "ID", "Detraccion"),
                new XElement(CbcNs + "PaymentMeansID", invoice.Detraccion.CodBienDetraccion),
                invoice.Detraccion.Porcentaje.HasValue
                    ? new XElement(CbcNs + "PaymentPercent", FormatAmount(invoice.Detraccion.Porcentaje.Value))
                    : null,
                new XElement(CbcNs + "Amount", new XAttribute("currencyID", currency), FormatAmount(invoice.Detraccion.Monto)))
        };
    }

    /// <summary>
    /// Forma de pago y cuotas - Paridad Greenter formaPago + cuotas
    /// </summary>
    private object? CreatePaymentTerms(Invoice invoice, string currency)
    {
        if (invoice.FormaPago == null) return null;

        var elements = new List<XElement>();

        // FormaPago principal (Contado o Credito)
        elements.Add(new XElement(CacNs + "PaymentTerms",
            new XElement(CbcNs + "ID", "FormaPago"),
            new XElement(CbcNs + "PaymentMeansID", invoice.FormaPago.Tipo ?? "Contado"),
            invoice.FormaPago.Monto.HasValue ?
                new XElement(CbcNs + "Amount",
                    new XAttribute("currencyID", invoice.FormaPago.Moneda ?? currency),
                    FormatAmount(invoice.FormaPago.Monto.Value)) : null
        ));

        // Cuotas (solo para Crédito)
        if (invoice.Cuotas != null)
        {
            int cuotaNum = 1;
            foreach (var cuota in invoice.Cuotas)
            {
                elements.Add(new XElement(CacNs + "PaymentTerms",
                    new XElement(CbcNs + "ID", "FormaPago"),
                    new XElement(CbcNs + "PaymentMeansID", $"Cuota{cuotaNum:D3}"),
                    new XElement(CbcNs + "Amount",
                        new XAttribute("currencyID", cuota.Moneda ?? currency),
                        FormatAmount(cuota.Monto)),
                    new XElement(CbcNs + "PaymentDueDate", cuota.FechaPago.ToString("yyyy-MM-dd"))
                ));
                cuotaNum++;
            }
        }

        return elements.ToArray();
    }

    private IEnumerable<XElement> CreatePrepayments(Invoice invoice)
    {
        return invoice.Anticipos!
            .Where(anticipo => anticipo.Total > 0)
            .Select((anticipo, index) =>
                new XElement(CacNs + "PrepaidPayment",
                    new XElement(CbcNs + "ID", index + 1),
                    new XElement(CbcNs + "PaidAmount", new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)), FormatAmount(anticipo.Total))));
    }

    /// <summary>
    /// AllowanceCharge globales (Greenter: cargos/descuentos a nivel de documento)
    /// </summary>
    private object? CreateGlobalCharges(List<Charge>? charges, bool isCharge, string currency)
    {
        if (charges == null || charges.Count == 0) return null;

        return charges.Select(c => new XElement(CacNs + "AllowanceCharge",
            new XElement(CbcNs + "ChargeIndicator", isCharge ? "true" : "false"),
            !string.IsNullOrEmpty(c.CodTipo) ?
                new XElement(CbcNs + "AllowanceChargeReasonCode", c.CodTipo) : null,
            c.Factor.HasValue ?
                new XElement(CbcNs + "MultiplierFactorNumeric", FormatAmount(c.Factor.Value)) : null,
            new XElement(CbcNs + "Amount",
                new XAttribute("currencyID", currency),
                FormatAmount(c.Monto ?? 0m)
            ),
            c.MontoBase.HasValue ?
                new XElement(CbcNs + "BaseAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(c.MontoBase.Value)
                ) : null
        )).ToArray();
    }

    /// <summary>
    /// Guías de remisión relacionadas (Greenter: guias → DespatchDocumentReference)
    /// </summary>
    private object? CreateRelatedDocuments(List<Document>? guias)
    {
        if (guias == null || guias.Count == 0) return null;

        return guias.Select(g => new XElement(CacNs + "DespatchDocumentReference",
            new XElement(CbcNs + "ID", g.NroDoc),
            new XElement(CbcNs + "DocumentTypeCode", g.TipoDoc)
        )).ToArray();
    }

    /// <summary>
    /// Documentos relacionados adicionales (Greenter: relDocs → AdditionalDocumentReference)
    /// </summary>
    private object? CreateAdditionalDocuments(List<Document>? relDocs)
    {
        if (relDocs == null || relDocs.Count == 0) return null;

        return relDocs.Select(d => new XElement(CacNs + "AdditionalDocumentReference",
            new XElement(CbcNs + "ID", d.NroDoc),
            new XElement(CbcNs + "DocumentTypeCode", d.TipoDoc)
        )).ToArray();
    }

    /// <summary>
    /// Seller supplier party (Greenter: seller - vendedor diferente al emisor)
    /// </summary>
    private XElement CreateSellerSupplierParty(Data.Entities.Client seller)
    {
        return new XElement(CacNs + "SellerSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", ((int)seller.TipoDoc).ToString()),
                        seller.NumDoc)
                ),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(seller.RznSocial))
                )
            )
        );
    }

    /// <summary>
    /// Delivery element (Greenter: dirección de entrega)
    /// </summary>
    private XElement CreateDelivery(Data.Entities.Address addr)
    {
        return new XElement(CacNs + "Delivery",
            new XElement(CacNs + "DeliveryLocation",
                new XElement(CacNs + "Address",
                    !string.IsNullOrEmpty(addr.Ubigeo) ?
                        new XElement(CbcNs + "ID", addr.Ubigeo) : null,
                    !string.IsNullOrEmpty(addr.Urbanizacion) ?
                        new XElement(CbcNs + "CitySubdivisionName", addr.Urbanizacion) : null,
                    !string.IsNullOrEmpty(addr.Provincia) ?
                        new XElement(CbcNs + "CityName", addr.Provincia) : null,
                    !string.IsNullOrEmpty(addr.Departamento) ?
                        new XElement(CbcNs + "CountrySubentity", addr.Departamento) : null,
                    !string.IsNullOrEmpty(addr.Distrito) ?
                        new XElement(CbcNs + "District", addr.Distrito) : null,
                    new XElement(CacNs + "AddressLine",
                        new XElement(CbcNs + "Line", new XCData(addr.Direccion))),
                    new XElement(CacNs + "Country",
                        new XElement(CbcNs + "IdentificationCode",
                            new XAttribute("listID", "ISO 3166-1"),
                            new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                            new XAttribute("listName", "Country"),
                            addr.CodigoPais ?? "PE")))));
    }

    /// <summary>
    /// Greenter: PaymentTerms ID=Percepcion con mtoTotal
    /// </summary>
    private XElement CreatePerceptionPaymentTerms(Invoice invoice)
    {
        return new XElement(CacNs + "PaymentTerms",
            new XElement(CbcNs + "ID", "Percepcion"),
            new XElement(CbcNs + "Amount",
                new XAttribute("currencyID", "PEN"),
                FormatAmount(invoice.Perception!.MtoTotal))
        );
    }

    /// <summary>
    /// Greenter: Perception as AllowanceCharge (ChargeIndicator=true)
    /// </summary>
    private XElement CreatePerceptionAllowanceCharge(Invoice invoice)
    {
        var perc = invoice.Perception!;
        return new XElement(CacNs + "AllowanceCharge",
            new XElement(CbcNs + "ChargeIndicator", "true"),
            new XElement(CbcNs + "AllowanceChargeReasonCode", perc.CodReg),
            new XElement(CbcNs + "MultiplierFactorNumeric", FormatAmount(perc.Porcentaje)),
            new XElement(CbcNs + "Amount",
                new XAttribute("currencyID", "PEN"),
                FormatAmount(perc.Mto)),
            new XElement(CbcNs + "BaseAmount",
                new XAttribute("currencyID", "PEN"),
                FormatAmount(perc.MtoBase))
        );
    }
}
