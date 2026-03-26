namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Clase base para generadores de XML UBL 2.1
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

    protected XElement CreateLegalMonetaryTotal(BaseSale sale)
    {
        return new XElement(CacNs + "LegalMonetaryTotal",
            new XElement(CbcNs + "LineExtensionAmount", 
                new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                sale.MtoOperGravadas.ToString("F2")
            ),
            new XElement(CbcNs + "TaxInclusiveAmount",
                new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                sale.MtoImpVenta.ToString("F2")
            ),
            new XElement(CbcNs + "AllowanceTotalAmount",
                new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                "0.00"
            ),
            new XElement(CbcNs + "PayableAmount",
                new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                sale.MtoImpVenta.ToString("F2")
            )
        );
    }

    protected XElement CreateTaxTotal(BaseSale sale)
    {
        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                sale.MtoIGV.ToString("F2")
            ),
            new XElement(CacNs + "TaxSubtotal",
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", GetCurrencyCode(sale.Moneda)),
                    sale.MtoIGV.ToString("F2")
                ),
                new XElement(CacNs + "TaxCategory",
                    new XElement(CbcNs + "TaxScheme",
                        new XElement(CbcNs + "ID", "1000"),
                        new XElement(CbcNs + "Name", "IGV"),
                        new XElement(CbcNs + "TaxTypeCode", "VAT")
                    )
                )
            )
        );
    }

    protected XElement CreateInvoiceLine(SaleDetail detail, int lineNumber, Currency currency)
    {
        return new XElement(CacNs + "InvoiceLine",
            new XElement(CbcNs + "ID", lineNumber),
            new XElement(CbcNs + "InvoicedQuantity",
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

    protected static string GetCurrencyCode(Currency currency) => currency switch
    {
        Currency.Pen => "PEN",
        Currency.Usd => "USD",
        Currency.Eur => "EUR",
        _ => "PEN"
    };
}
