namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using System.Globalization;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Clase base para generadores de XML UBL 2.1 - Paridad 100% Greenter
/// </summary>
public abstract class XmlBuilderBase
{
    protected static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    protected static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    protected static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    protected static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    protected XElement CreateSignature(Company company)
    {
        return new XElement(CacNs + "Signature",
            new XElement(CbcNs + "ID", $"SIGN{company.Ruc}"),
            new XElement(CacNs + "SignatoryParty",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID", company.Ruc)
                ),
                new XElement(CacNs + "PartyName",
                    new XElement(CbcNs + "Name", new XCData(company.RazonSocial))
                )
            ),
            new XElement(CacNs + "DigitalSignatureAttachment",
                new XElement(CacNs + "ExternalReference",
                    new XElement(CbcNs + "URI", "#GREENTER-SIGN")
                )
            )
        );
    }

    protected XElement CreateSupplierParty(Company company)
    {
        var addr = company.Address;

        return new XElement(CacNs + "AccountingSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", "6"),
                        company.Ruc
                    )
                ),
                !string.IsNullOrEmpty(company.NombreComercial) ?
                    new XElement(CacNs + "PartyName",
                        new XElement(CbcNs + "Name", new XCData(company.NombreComercial))
                    ) : null,
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(company.RazonSocial)),
                    CreateRegistrationAddress(addr)
                )
            )
        );
    }

    protected XElement CreateCustomerParty(Client client)
    {
        var addr = client.Address;

        return new XElement(CacNs + "AccountingCustomerParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", ((int)client.TipoDoc).ToString()),
                        client.NumDoc
                    )
                ),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(client.RznSocial)),
                    addr != null ? new XElement(CacNs + "RegistrationAddress",
                        !string.IsNullOrEmpty(addr.Ubigeo) ?
                            new XElement(CbcNs + "ID", addr.Ubigeo) : null,
                        new XElement(CacNs + "AddressLine",
                            new XElement(CbcNs + "Line", new XCData(addr.Direccion))
                        ),
                        new XElement(CacNs + "Country",
                            new XElement(CbcNs + "IdentificationCode", "PE")
                        )
                    ) : null
                )
            )
        );
    }

    protected XElement CreateRegistrationAddress(Address addr)
    {
        return new XElement(CacNs + "RegistrationAddress",
            new XElement(CbcNs + "ID", addr.Ubigeo),
            new XElement(CbcNs + "AddressTypeCode", addr.CodigoLocal),
            !string.IsNullOrEmpty(addr.Urbanizacion) ?
                new XElement(CbcNs + "CitySubdivisionName", addr.Urbanizacion) : null,
            new XElement(CbcNs + "CityName", addr.Provincia),
            new XElement(CbcNs + "CountrySubentity", addr.Departamento),
            new XElement(CbcNs + "District", addr.Distrito),
            new XElement(CacNs + "AddressLine",
                new XElement(CbcNs + "Line", new XCData(addr.Direccion))
            ),
            new XElement(CacNs + "Country",
                new XElement(CbcNs + "IdentificationCode", "PE")
            )
        );
    }

    /// <summary>
    /// LegalMonetaryTotal para Invoice - Paridad exacta Greenter invoice2.1.xml.twig líneas 415-433
    /// Greenter usa doc.valorVenta para LineExtensionAmount, doc.subTotal para TaxInclusiveAmount, etc.
    /// </summary>
    protected XElement CreateInvoiceLegalMonetaryTotal(Invoice invoice)
    {
        var currency = GetCurrencyCode(invoice.Moneda);

        // Greenter: LineExtensionAmount = doc.valorVenta
        var lineExtension = invoice.ValorVenta ??
            (invoice.MtoOperGravadas + invoice.MtoOperExoneradas +
             invoice.MtoOperInafectas + invoice.MtoOperExportacion);

        return new XElement(CacNs + "LegalMonetaryTotal",
            new XElement(CbcNs + "LineExtensionAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(lineExtension)
            ),
            // Greenter: {% if doc.subTotal is not null %}
            invoice.SubTotal.HasValue ?
                new XElement(CbcNs + "TaxInclusiveAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(invoice.SubTotal.Value)
                ) : null,
            // Greenter: {% if doc.sumOtrosDescuentos is not null %}
            invoice.SumOtrosDescuentos.HasValue ?
                new XElement(CbcNs + "AllowanceTotalAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(invoice.SumOtrosDescuentos.Value)
                ) : null,
            // Greenter: {% if doc.sumOtrosCargos is not null %}
            invoice.SumOtrosCargos.HasValue ?
                new XElement(CbcNs + "ChargeTotalAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(invoice.SumOtrosCargos.Value)
                ) : null,
            // Greenter: {% if doc.totalAnticipos is not null %}
            invoice.TotalAnticipos.HasValue ?
                new XElement(CbcNs + "PrepaidAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(invoice.TotalAnticipos.Value)
                ) : null,
            // Greenter: {% if doc.redondeo is not null %}
            invoice.Redondeo != 0 ?
                new XElement(CbcNs + "PayableRoundingAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(invoice.Redondeo)
                ) : null,
            new XElement(CbcNs + "PayableAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(invoice.MtoImpVenta)
            )
        );
    }

    /// <summary>
    /// LegalMonetaryTotal genérico para CreditNote/otros - Greenter CreditNote solo usa:
    /// ChargeTotalAmount, PayableRoundingAmount, PayableAmount
    /// </summary>
    protected XElement CreateLegalMonetaryTotal(BaseSale sale)
    {
        var currency = GetCurrencyCode(sale.Moneda);
        var lineExtension = sale.MtoOperGravadas + sale.MtoOperExoneradas +
                            sale.MtoOperInafectas + sale.MtoOperExportacion;

        return new XElement(CacNs + "LegalMonetaryTotal",
            // Greenter CreditNote: solo ChargeTotalAmount (opcional), PayableRoundingAmount, PayableAmount
            sale.SumOtrosCargos.HasValue ?
                new XElement(CbcNs + "ChargeTotalAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(sale.SumOtrosCargos.Value)
                ) : null,
            sale.Redondeo != 0 ?
                new XElement(CbcNs + "PayableRoundingAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(sale.Redondeo)
                ) : null,
            new XElement(CbcNs + "PayableAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(sale.MtoImpVenta)
            )
        );
    }

    /// <summary>
    /// TaxTotal con todos los subtotales - Paridad exacta Greenter invoice2.1.xml.twig líneas 296-414
    /// Greenter genera UN SOLO TaxTotal con TODOS los subtotales adentro (incluye INA, EXO, GRA, EXP)
    /// Orden: ISC → IGV → INA → EXO → GRA → EXP → IVAP → OTROS → ICBPER
    /// </summary>
    protected XElement CreateTaxTotals(BaseSale sale)
    {
        var currency = GetCurrencyCode(sale.Moneda);
        var subtotals = new List<XElement>();

        // ISC (si mtoISC > 0)
        if (sale.MtoISC > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoBaseIsc, sale.MtoISC, "2000", "ISC", "EXC", currency));

        // IGV (si mtoOperGravadas is not null - Greenter: {% if doc.mtoOperGravadas is not null %})
        if (sale.MtoOperGravadas > 0 || sale.MtoIGV > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoOperGravadas, sale.MtoIGV, "1000", "IGV", "VAT", currency));

        // INA (si mtoOperInafectas is not null - TaxAmount siempre 0)
        if (sale.MtoOperInafectas > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoOperInafectas, 0m, "9998", "INA", "FRE", currency));

        // EXO (si mtoOperExoneradas is not null - TaxAmount siempre 0)
        if (sale.MtoOperExoneradas > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoOperExoneradas, 0m, "9997", "EXO", "VAT", currency));

        // GRA (si mtoOperGratuitas is not null - DENTRO del mismo TaxTotal, no separado)
        if (sale.MtoOperGratuitas > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoOperGratuitas, sale.MtoIGVGratuitas, "9996", "GRA", "FRE", currency));

        // EXP (si mtoOperExportacion is not null - TaxAmount siempre 0)
        if (sale.MtoOperExportacion > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoOperExportacion, 0m, "9995", "EXP", "FRE", currency));

        // IVAP (si mtoIvap > 0)
        if (sale.MtoIvap > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoBaseIvap, sale.MtoIvap, "1016", "IVAP", "VAT", currency));

        // OTROS (si mtoOtrosTributos > 0)
        if (sale.MtoOtrosTributos > 0)
            subtotals.Add(CreateTaxSubtotal(sale.MtoBaseOth, sale.MtoOtrosTributos, "9999", "OTROS", "OTH", currency));

        // ICBPER (si icbper > 0) - sin TaxableAmount
        if (sale.Icbper > 0)
            subtotals.Add(CreateIcbperSubtotal(sale.Icbper, currency));

        // Fallback: si TotalImpuestos no fue calculado, sumar componentes
        var totalTax = sale.TotalImpuestos > 0
            ? sale.TotalImpuestos
            : sale.MtoIGV + sale.MtoISC + sale.MtoIvap + sale.MtoOtrosTributos + sale.Icbper;

        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(totalTax)
            ),
            subtotals
        );
    }

    /// <summary>
    /// ICBPER header subtotal - Greenter no incluye TaxableAmount, solo TaxAmount
    /// </summary>
    private XElement CreateIcbperSubtotal(decimal amount, string currency)
    {
        return new XElement(CacNs + "TaxSubtotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(amount)
            ),
            new XElement(CacNs + "TaxCategory",
                new XElement(CbcNs + "TaxScheme",
                    new XElement(CbcNs + "ID", "7152"),
                    new XElement(CbcNs + "Name", "ICBPER"),
                    new XElement(CbcNs + "TaxTypeCode", "OTH")
                )
            )
        );
    }

    /// <summary>
    /// Backward-compatible single TaxTotal (solo IGV) para código existente
    /// </summary>
    protected XElement CreateTaxTotal(BaseSale sale)
    {
        var currency = GetCurrencyCode(sale.Moneda);
        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(sale.TotalImpuestos > 0 ? sale.TotalImpuestos : sale.MtoIGV)
            ),
            CreateTaxSubtotal(sale.MtoOperGravadas, sale.MtoIGV, "1000", "IGV", "VAT", currency),
            sale.MtoISC > 0 ? CreateTaxSubtotal(sale.MtoBaseIsc, sale.MtoISC, "2000", "ISC", "EXC", currency) : null,
            sale.MtoIvap > 0 ? CreateTaxSubtotal(sale.MtoBaseIvap, sale.MtoIvap, "1016", "IVAP", "VAT", currency) : null,
            sale.MtoOtrosTributos > 0 ? CreateTaxSubtotal(sale.MtoBaseOth, sale.MtoOtrosTributos, "9999", "OTROS", "OTH", currency) : null,
            sale.Icbper > 0 ? CreateTaxSubtotal(0m, sale.Icbper, "7152", "ICBPER", "OTH", currency) : null
        );
    }

    private XElement CreateTaxSubtotal(decimal taxableAmount, decimal taxAmount,
        string schemeId, string schemeName, string typeCode, string currency)
    {
        return new XElement(CacNs + "TaxSubtotal",
            taxableAmount > 0 ? new XElement(CbcNs + "TaxableAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(taxableAmount)
            ) : null,
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(taxAmount)
            ),
            new XElement(CacNs + "TaxCategory",
                new XElement(CbcNs + "TaxScheme",
                    new XElement(CbcNs + "ID", schemeId),
                    new XElement(CbcNs + "Name", schemeName),
                    new XElement(CbcNs + "TaxTypeCode", typeCode)
                )
            )
        );
    }

    /// <summary>
    /// InvoiceLine con soporte multi-impuesto - Paridad Greenter
    /// </summary>
    protected XElement CreateInvoiceLine(SaleDetail detail, int lineNumber, Currency currency)
    {
        var currencyCode = GetCurrencyCode(currency);
        var tributo = Khipu.Data.Algorithms.TributoFunction.GetByAfectacion(detail.TipoAfectacionIgv);

        // Calcular impuestos de línea
        var igvAmount = detail.Igv ?? (detail.MtoValorVenta * detail.TasaIgv);
        var baseIgv = detail.MtoBaseIgv ?? detail.MtoValorVenta;

        // PricingReference: precio con impuestos o valor gratuito
        var isGratuita = detail.TipoAfectacionIgv == TaxType.Gratuito;
        var pricingRef = CreatePricingReference(detail, currencyCode, isGratuita);

        var lineTaxes = new List<XElement>();

        // ISC (antes de IGV, como Greenter) - incluye Percent y TierRange
        if (detail.MtoIsc.HasValue && detail.MtoIsc > 0)
        {
            lineTaxes.Add(CreateLineIscSubtotal(detail, currencyCode));
        }

        // IGV/IVAP/Exonerado/Inafecto/Exportación/Gratuito
        var afectCode = ((int)detail.TipoAfectacionIgv).ToString();
        var igvPercent = detail.PorcentajeIgv ?? (detail.TasaIgv * 100m);
        if (tributo != null)
        {
            lineTaxes.Add(CreateLineTaxSubtotal(baseIgv, igvAmount,
                tributo.Id, tributo.Name, tributo.Code, currencyCode, afectCode, igvPercent));
        }
        else
        {
            lineTaxes.Add(CreateLineTaxSubtotal(baseIgv, igvAmount,
                "1000", "IGV", "VAT", currencyCode, afectCode, igvPercent));
        }

        // Otros Tributos
        if (detail.OtroTributo.HasValue && detail.OtroTributo > 0)
        {
            lineTaxes.Add(CreateLineTaxSubtotal(
                detail.MtoBaseOth ?? detail.MtoValorVenta, detail.OtroTributo.Value,
                "9999", "OTROS", "OTH", currencyCode));
        }

        // ICBPER - Greenter usa PerUnitAmount + BaseUnitMeasure (no TaxableAmount)
        if (detail.Icbper.HasValue && detail.Icbper > 0)
        {
            lineTaxes.Add(CreateLineIcbperSubtotal(detail, currencyCode));
        }

        var totalLineTax = igvAmount + (detail.MtoIsc ?? 0m) + (detail.OtroTributo ?? 0m) + (detail.Icbper ?? 0m);

        return new XElement(CacNs + "InvoiceLine",
            new XElement(CbcNs + "ID", lineNumber),
            new XElement(CbcNs + "InvoicedQuantity",
                new XAttribute("unitCode", detail.Unidad),
                detail.Cantidad.ToString("F2")
            ),
            new XElement(CbcNs + "LineExtensionAmount",
                new XAttribute("currencyID", currencyCode),
                FormatAmount(detail.MtoValorVenta)
            ),
            pricingRef,
            // Line-level charges/discounts (Greenter: cargos/descuentos de línea)
            CreateLineCharges(detail.Descuentos, false, currencyCode),
            CreateLineCharges(detail.Cargos, true, currencyCode),
            new XElement(CacNs + "TaxTotal",
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(totalLineTax)
                ),
                lineTaxes
            ),
            new XElement(CacNs + "Item",
                new XElement(CbcNs + "Description", new XCData(detail.Descripcion)),
                new XElement(CacNs + "SellersItemIdentification",
                    new XElement(CbcNs + "ID", detail.Codigo)
                ),
                !string.IsNullOrEmpty(detail.CodProdSunat) ?
                    new XElement(CacNs + "CommodityClassification",
                        new XElement(CbcNs + "ItemClassificationCode",
                            new XAttribute("listID", "UNSPSC"),
                            detail.CodProdSunat)
                    ) : null,
                !string.IsNullOrEmpty(detail.CodProdGS1) ?
                    new XElement(CacNs + "StandardItemIdentification",
                        new XElement(CbcNs + "ID",
                            new XAttribute("schemeID", "0160"),
                            detail.CodProdGS1)
                    ) : null
            ),
            new XElement(CacNs + "Price",
                new XElement(CbcNs + "PriceAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(detail.MtoValorUnitario)
                )
            )
        );
    }

    private XElement CreatePricingReference(SaleDetail detail, string currencyCode, bool isGratuita)
    {
        var elements = new List<XElement>();

        if (!isGratuita)
        {
            // Greenter: usa mtoPrecioUnitario (precio unitario con impuestos)
            var unitPrice = detail.MtoPrecioUnitario ?? detail.PrecioVenta;
            elements.Add(new XElement(CacNs + "AlternativeConditionPrice",
                new XElement(CbcNs + "PriceAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(unitPrice)
                ),
                new XElement(CbcNs + "PriceTypeCode", "01")
            ));
        }
        else
        {
            // Greenter: para gratuitas, PriceTypeCode = "02" con valor referencial
            elements.Add(new XElement(CacNs + "AlternativeConditionPrice",
                new XElement(CbcNs + "PriceAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(detail.MtoValorGratuito ?? detail.PrecioVenta)
                ),
                new XElement(CbcNs + "PriceTypeCode", "02")
            ));
        }

        return new XElement(CacNs + "PricingReference", elements);
    }

    private XElement CreateLineTaxSubtotal(decimal taxableAmount, decimal taxAmount,
        string schemeId, string schemeName, string typeCode, string currency,
        string? afectacionCode = null, decimal? percent = null)
    {
        // Porcentaje: si se pasa explícitamente se usa, sino se infiere del schemeId
        var pct = percent ?? schemeId switch
        {
            "1000" => 18.00m,
            "1016" => 4.00m,
            "7152" => 0m,
            _ => 0m
        };

        return new XElement(CacNs + "TaxSubtotal",
            new XElement(CbcNs + "TaxableAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(taxableAmount)
            ),
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(taxAmount)
            ),
            new XElement(CacNs + "TaxCategory",
                new XElement(CbcNs + "Percent", FormatAmount(pct)),
                !string.IsNullOrEmpty(afectacionCode) ?
                    new XElement(CbcNs + "TaxExemptionReasonCode", afectacionCode) : null,
                new XElement(CbcNs + "TaxScheme",
                    new XElement(CbcNs + "ID", schemeId),
                    new XElement(CbcNs + "Name", schemeName),
                    new XElement(CbcNs + "TaxTypeCode", typeCode)
                )
            )
        );
    }

    /// <summary>
    /// AllowanceCharge elements para cargos/descuentos de línea (Greenter)
    /// </summary>
    protected object? CreateLineCharges(List<Charge>? charges, bool isCharge, string currencyCode)
    {
        if (charges == null || charges.Count == 0) return null;

        return charges.Select(c => new XElement(CacNs + "AllowanceCharge",
            new XElement(CbcNs + "ChargeIndicator", isCharge ? "true" : "false"),
            !string.IsNullOrEmpty(c.CodTipo) ?
                new XElement(CbcNs + "AllowanceChargeReasonCode", c.CodTipo) : null,
            c.Factor.HasValue ?
                new XElement(CbcNs + "MultiplierFactorNumeric", FormatAmount(c.Factor.Value)) : null,
            new XElement(CbcNs + "Amount",
                new XAttribute("currencyID", currencyCode),
                FormatAmount(c.Monto ?? 0m)
            ),
            c.MontoBase.HasValue ?
                new XElement(CbcNs + "BaseAmount",
                    new XAttribute("currencyID", currencyCode),
                    FormatAmount(c.MontoBase.Value)
                ) : null
        )).ToArray();
    }

    /// <summary>
    /// ISC line subtotal - Greenter incluye Percent y TierRange (tipSisIsc)
    /// </summary>
    private XElement CreateLineIscSubtotal(SaleDetail detail, string currency)
    {
        return new XElement(CacNs + "TaxSubtotal",
            new XElement(CbcNs + "TaxableAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(detail.MtoBaseIsc ?? detail.MtoValorVenta)
            ),
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(detail.MtoIsc!.Value)
            ),
            new XElement(CacNs + "TaxCategory",
                detail.PorcentajeIsc.HasValue ?
                    new XElement(CbcNs + "Percent", FormatAmount(detail.PorcentajeIsc.Value)) : null,
                !string.IsNullOrEmpty(detail.TipSisIsc) ?
                    new XElement(CbcNs + "TierRange", detail.TipSisIsc) : null,
                new XElement(CbcNs + "TaxScheme",
                    new XElement(CbcNs + "ID", "2000"),
                    new XElement(CbcNs + "Name", "ISC"),
                    new XElement(CbcNs + "TaxTypeCode", "EXC")
                )
            )
        );
    }

    /// <summary>
    /// ICBPER line subtotal - Greenter usa PerUnitAmount + BaseUnitMeasure (NO TaxableAmount)
    /// </summary>
    private XElement CreateLineIcbperSubtotal(SaleDetail detail, string currency)
    {
        return new XElement(CacNs + "TaxSubtotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(detail.Icbper!.Value)
            ),
            new XElement(CacNs + "TaxCategory",
                new XElement(CbcNs + "PerUnitAmount",
                    new XAttribute("currencyID", currency),
                    FormatAmount(detail.FactorIcbper)
                ),
                new XElement(CbcNs + "BaseUnitMeasure",
                    new XAttribute("unitCode", detail.Unidad),
                    detail.Cantidad.ToString("F0")
                ),
                new XElement(CbcNs + "TaxScheme",
                    new XElement(CbcNs + "ID", "7152"),
                    new XElement(CbcNs + "Name", "ICBPER"),
                    new XElement(CbcNs + "TaxTypeCode", "OTH")
                )
            )
        );
    }

    protected static string FormatAmount(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    protected static string GetCurrencyCode(Currency currency) => currency switch
    {
        Currency.Pen => "PEN",
        Currency.Usd => "USD",
        Currency.Eur => "EUR",
        _ => "PEN"
    };
}
