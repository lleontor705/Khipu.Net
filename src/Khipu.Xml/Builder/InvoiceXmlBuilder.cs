namespace Khipu.Xml.Builder;

using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Enums;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Facturas
/// </summary>
public class InvoiceXmlBuilder : IXmlBuilder<Invoice>
{
    private static readonly XNamespace InvoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Invoice document)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateInvoiceElement(document)
        );
        
        return doc.ToString();
    }

    public string GetFileName(Invoice document)
    {
        var ruc = document.Company.Ruc;
        var tipoDoc = ((int)document.TipoDoc).ToString().PadLeft(2, '0');
        var serie = document.Serie;
        var correlativo = document.Correlativo.ToString().PadLeft(8, '0');
        return $"{ruc}-{tipoDoc}-{serie}-{correlativo}.xml";
    }

    private XElement CreateInvoiceElement(Invoice invoice)
    {
        return new XElement(InvoiceNs + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            
            // UBL Extensions (para firma digital)
            new XElement(ExtNs + "UBLExtensions",
                new XElement(ExtNs + "UBLExtension",
                    new XElement(ExtNs + "ExtensionContent")
                )
            ),
            
            // UBL Version
            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "2.0"),
            
            // ID del documento
            new XElement(CbcNs + "ID", $"{invoice.Serie}-{invoice.Correlativo}"),
            
            // Fechas
            new XElement(CbcNs + "IssueDate", invoice.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", invoice.FechaEmision.ToString("HH:mm:ss")),
            
            // Tipo de documento
            new XElement(CbcNs + "InvoiceTypeCode", 
                new XAttribute("listID", invoice.TipoOperacion ?? "0101"),
                ((int)invoice.TipoDoc).ToString().PadLeft(2, '0')
            ),
            
            // Moneda
            new XElement(CbcNs + "DocumentCurrencyCode", GetCurrencyCode(invoice.Moneda)),
            
            // Firma
            CreateSignature(invoice),
            
            // Emisor
            CreateSupplierParty(invoice),
            
            // Receptor
            CreateCustomerParty(invoice),
            
            // Totales
            CreateLegalMonetaryTotal(invoice),
            
            // Tax Total
            CreateTaxTotal(invoice),
            
            // Detalles
            invoice.Details.Select((d, i) => CreateInvoiceLine(d, i + 1))
        );
    }

    private XElement CreateSignature(Invoice invoice)
    {
        var emp = invoice.Company;
        return new XElement(CacNs + "Signature",
            new XElement(CbcNs + "ID", $"SIGN{emp.Ruc}"),
            new XElement(CacNs + "SignatoryParty",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID", emp.Ruc)
                ),
                new XElement(CacNs + "PartyName",
                    new XElement(CbcNs + "Name", new XCData(emp.RazonSocial))
                )
            ),
            new XElement(CacNs + "DigitalSignatureAttachment",
                new XElement(CacNs + "ExternalReference",
                    new XElement(CbcNs + "URI", "#GREENTER-SIGN")
                )
            )
        );
    }

    private XElement CreateSupplierParty(Invoice invoice)
    {
        var emp = invoice.Company;
        var addr = emp.Address;
        
        return new XElement(CacNs + "AccountingSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID", 
                        new XAttribute("schemeID", "6"),
                        emp.Ruc
                    )
                ),
                !string.IsNullOrEmpty(emp.NombreComercial) ?
                    new XElement(CacNs + "PartyName",
                        new XElement(CbcNs + "Name", new XCData(emp.NombreComercial))
                    ) : null,
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(emp.RazonSocial)),
                    new XElement(CacNs + "RegistrationAddress",
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
                    )
                )
            )
        );
    }

    private XElement CreateCustomerParty(Invoice invoice)
    {
        var client = invoice.Client;
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

    private XElement CreateLegalMonetaryTotal(Invoice invoice)
    {
        return new XElement(CacNs + "LegalMonetaryTotal",
            new XElement(CbcNs + "LineExtensionAmount", 
                new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                invoice.MtoOperGravadas.ToString("F2")
            ),
            new XElement(CbcNs + "TaxInclusiveAmount",
                new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                invoice.MtoImpVenta.ToString("F2")
            ),
            new XElement(CbcNs + "AllowanceTotalAmount",
                new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                (invoice.MtoDescuentos ?? 0).ToString("F2")
            ),
            new XElement(CbcNs + "PayableAmount",
                new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                invoice.MtoImpVenta.ToString("F2")
            )
        );
    }

    private XElement CreateTaxTotal(Invoice invoice)
    {
        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                invoice.MtoIGV.ToString("F2")
            ),
            new XElement(CacNs + "TaxSubtotal",
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", GetCurrencyCode(invoice.Moneda)),
                    invoice.MtoIGV.ToString("F2")
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

    private XElement CreateInvoiceLine(SaleDetail detail, int lineNumber)
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
                        new XAttribute("currencyID", "PEN"),
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

    private static string GetCurrencyCode(Currency currency) => currency switch
    {
        Currency.Pen => "PEN",
        Currency.Usd => "USD",
        Currency.Eur => "EUR",
        _ => "PEN"
    };
}

