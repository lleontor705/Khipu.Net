namespace Khipu.Xml.Parser;

using System.Xml.Linq;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Parser de XML UBL para documentos SUNAT - Paridad Greenter xml-parser package.
/// Convierte XML de comprobantes electrónicos de vuelta a objetos del modelo.
/// </summary>
public static class XmlDocumentParser
{
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace SacNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";

    /// <summary>
    /// Parsea un XML de factura UBL 2.1 a un objeto Invoice.
    /// </summary>
    public static Invoice? ParseInvoice(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var invoice = new Invoice
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            Moneda = ParseCurrency(root.Descendants(CbcNs + "DocumentCurrencyCode").FirstOrDefault()?.Value),
            Company = ParseSupplierParty(root),
            Client = ParseCustomerParty(root)
        };

        // Totales
        var lmt = root.Element(CacNs + "LegalMonetaryTotal");
        if (lmt != null)
        {
            invoice.MtoImpVenta = ParseDecimal(lmt.Element(CbcNs + "PayableAmount")?.Value);
            invoice.ValorVenta = ParseDecimal(lmt.Element(CbcNs + "LineExtensionAmount")?.Value);
            invoice.SubTotal = ParseDecimalNullable(lmt.Element(CbcNs + "TaxInclusiveAmount")?.Value);
            invoice.SumOtrosDescuentos = ParseDecimalNullable(lmt.Element(CbcNs + "AllowanceTotalAmount")?.Value);
            invoice.SumOtrosCargos = ParseDecimalNullable(lmt.Element(CbcNs + "ChargeTotalAmount")?.Value);
            invoice.TotalAnticipos = ParseDecimalNullable(lmt.Element(CbcNs + "PrepaidAmount")?.Value);
        }

        // Impuestos
        ParseTaxTotals(root, invoice);

        // Leyendas (Note elements)
        invoice.Leyendas = root.Elements(CbcNs + "Note")
            .Select(n => new Legend
            {
                Code = n.Attribute("languageLocaleID")?.Value ?? "",
                Value = n.Value
            }).ToList();

        // Detalle
        invoice.Details = root.Elements(CacNs + "InvoiceLine")
            .Select(ParseSaleDetail)
            .ToList();

        return invoice;
    }

    /// <summary>
    /// Parsea un XML de nota de crédito UBL 2.1.
    /// </summary>
    public static CreditNote? ParseCreditNote(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var note = new CreditNote
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            Moneda = ParseCurrency(root.Descendants(CbcNs + "DocumentCurrencyCode").FirstOrDefault()?.Value),
            Company = ParseSupplierParty(root),
            Client = ParseCustomerParty(root)
        };

        // Discrepancy Response
        var discrepancy = root.Element(CacNs + "DiscrepancyResponse");
        if (discrepancy != null)
        {
            note.CodMotivo = GetValue(discrepancy, CbcNs + "ResponseCode") ?? "";
            note.DesMotivo = GetValue(discrepancy, CbcNs + "Description") ?? "";
        }

        // Billing Reference
        var billingRef = root.Element(CacNs + "BillingReference")
            ?.Element(CacNs + "InvoiceDocumentReference");
        if (billingRef != null)
        {
            note.NumDocAfectado = GetValue(billingRef, CbcNs + "ID") ?? "";
            note.TipDocAfectado = GetValue(billingRef, CbcNs + "DocumentTypeCode") ?? "";
        }

        // Totales
        var lmt = root.Element(CacNs + "LegalMonetaryTotal");
        if (lmt != null)
        {
            note.MtoImpVenta = ParseDecimal(lmt.Element(CbcNs + "PayableAmount")?.Value);
        }

        ParseTaxTotals(root, note);

        note.Details = root.Elements(CacNs + "CreditNoteLine")
            .Select(ParseSaleDetail)
            .ToList();

        return note;
    }

    /// <summary>
    /// Parsea un XML de nota de débito UBL 2.1.
    /// </summary>
    public static DebitNote? ParseDebitNote(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var note = new DebitNote
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            Moneda = ParseCurrency(root.Descendants(CbcNs + "DocumentCurrencyCode").FirstOrDefault()?.Value),
            Company = ParseSupplierParty(root),
            Client = ParseCustomerParty(root)
        };

        var discrepancy = root.Element(CacNs + "DiscrepancyResponse");
        if (discrepancy != null)
        {
            note.CodMotivo = GetValue(discrepancy, CbcNs + "ResponseCode") ?? "";
            note.DesMotivo = GetValue(discrepancy, CbcNs + "Description") ?? "";
        }

        var billingRef = root.Element(CacNs + "BillingReference")
            ?.Element(CacNs + "InvoiceDocumentReference");
        if (billingRef != null)
        {
            note.NumDocAfectado = GetValue(billingRef, CbcNs + "ID") ?? "";
            note.TipDocAfectado = GetValue(billingRef, CbcNs + "DocumentTypeCode") ?? "";
        }

        var rmt = root.Element(CacNs + "RequestedMonetaryTotal");
        if (rmt != null)
        {
            note.MtoImpVenta = ParseDecimal(rmt.Element(CbcNs + "PayableAmount")?.Value);
        }

        ParseTaxTotals(root, note);

        note.Details = root.Elements(CacNs + "DebitNoteLine")
            .Select(ParseSaleDetail)
            .ToList();

        return note;
    }

    /// <summary>
    /// Parsea un XML de guía de remisión.
    /// </summary>
    public static Despatch? ParseDespatch(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var despatch = new Despatch
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            DesMotivoTraslado = GetValue(root, CbcNs + "Note") ?? ""
        };

        // Supplier (emisor)
        var supplier = root.Element(CacNs + "DespatchSupplierParty")?.Element(CacNs + "Party");
        if (supplier != null)
        {
            despatch.Company = new Company
            {
                Ruc = supplier.Descendants(CbcNs + "ID").FirstOrDefault()?.Value ?? "",
                RazonSocial = supplier.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        // Customer (destinatario)
        var customer = root.Element(CacNs + "DeliveryCustomerParty")?.Element(CacNs + "Party");
        if (customer != null)
        {
            despatch.Destinatario = new Client
            {
                NumDoc = customer.Descendants(CbcNs + "ID").FirstOrDefault()?.Value ?? "",
                RznSocial = customer.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        // Shipment
        var shipment = root.Element(CacNs + "Shipment");
        if (shipment != null)
        {
            despatch.CodMotivoTraslado = GetValue(shipment, CbcNs + "HandlingCode") ?? "";
            var weight = shipment.Element(CbcNs + "GrossWeightMeasure");
            if (weight != null)
            {
                despatch.PesoTotal = ParseDecimalNullable(weight.Value);
                despatch.UndPesoTotal = weight.Attribute("unitCode")?.Value;
            }

            // Addresses
            var delivery = shipment.Element(CacNs + "Delivery")?.Element(CacNs + "DeliveryAddress");
            if (delivery != null) despatch.PuntoLlegada = ParseAddress(delivery);

            var origin = shipment.Element(CacNs + "OriginAddress");
            if (origin != null) despatch.PuntoPartida = ParseAddress(origin);
        }

        // Details
        despatch.Details = root.Elements(CacNs + "DespatchLine")
            .Select((el, i) =>
            {
                var qty = el.Element(CbcNs + "DeliveredQuantity");
                var item = el.Element(CacNs + "Item");
                return new DespatchDetail
                {
                    Orden = i + 1,
                    Cantidad = ParseDecimal(qty?.Value),
                    Unidad = qty?.Attribute("unitCode")?.Value ?? "NIU",
                    Descripcion = item?.Element(CbcNs + "Description")?.Value ?? "",
                    Codigo = item?.Descendants(CbcNs + "ID").FirstOrDefault()?.Value ?? "",
                    CodProdSunat = item?.Descendants(CbcNs + "ItemClassificationCode").FirstOrDefault()?.Value
                };
            }).ToList();

        return despatch;
    }

    /// <summary>
    /// Parsea un XML de percepción SUNAT.
    /// </summary>
    public static Perception? ParsePerception(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var perception = new Perception
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            MtoPercepcion = ParseDecimal(root.Element(CbcNs + "TotalInvoiceAmount")?.Value),
            MtoTotalCobrar = ParseDecimal(root.Element(SacNs + "SUNATTotalCashed")?.Value)
        };

        // Agent (emisor)
        var agent = root.Element(CacNs + "AgentParty");
        if (agent != null)
        {
            perception.Company = new Company
            {
                Ruc = agent.Descendants(CbcNs + "ID").FirstOrDefault()?.Value ?? "",
                RazonSocial = agent.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        // Receiver (proveedor)
        var receiver = root.Element(CacNs + "ReceiverParty");
        if (receiver != null)
        {
            var idEl = receiver.Element(CacNs + "PartyIdentification")?.Element(CbcNs + "ID");
            perception.Proveedor = new Client
            {
                NumDoc = idEl?.Value ?? "",
                TipoDoc = ParseDocumentType(idEl?.Attribute("schemeID")?.Value),
                RznSocial = receiver.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value
                    ?? receiver.Descendants(CbcNs + "Name").FirstOrDefault()?.Value ?? ""
            };
        }

        // Details
        perception.Details = root.Elements(SacNs + "SUNATPerceptionDocumentReference")
            .Select((el, i) =>
            {
                var idEl = el.Element(CbcNs + "ID");
                var info = el.Element(SacNs + "SUNATPerceptionInformation");
                return new PerceptionDetail
                {
                    Orden = i + 1,
                    TipoDoc = idEl?.Attribute("schemeID")?.Value ?? "",
                    NumDoc = idEl?.Value ?? "",
                    FechaEmision = ParseDate(GetValue(el, CbcNs + "IssueDate")),
                    ImpTotal = ParseDecimal(el.Element(CbcNs + "TotalInvoiceAmount")?.Value),
                    CodMoneda = el.Element(CbcNs + "TotalInvoiceAmount")?.Attribute("currencyID")?.Value ?? "PEN",
                    Mto = ParseDecimal(info?.Element(SacNs + "SUNATPerceptionAmount")?.Value),
                    ImpCobrar = ParseDecimal(info?.Element(SacNs + "SUNATNetTotalCashed")?.Value)
                };
            }).ToList();

        return perception;
    }

    /// <summary>
    /// Parsea un XML de retención SUNAT.
    /// </summary>
    public static Retention? ParseRetention(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var id = GetValue(root, CbcNs + "ID");
        var (serie, correlativo) = SplitId(id);

        var retention = new Retention
        {
            Serie = serie,
            Correlativo = int.TryParse(correlativo, out var c) ? c : 0,
            FechaEmision = ParseDate(GetValue(root, CbcNs + "IssueDate")),
            MtoRetencion = ParseDecimal(root.Element(CbcNs + "TotalInvoiceAmount")?.Value),
            MtoTotal = ParseDecimal(root.Element(SacNs + "SUNATTotalPaid")?.Value)
        };

        var agent = root.Element(CacNs + "AgentParty");
        if (agent != null)
        {
            retention.Company = new Company
            {
                Ruc = agent.Descendants(CbcNs + "ID").FirstOrDefault()?.Value ?? "",
                RazonSocial = agent.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        var receiver = root.Element(CacNs + "ReceiverParty");
        if (receiver != null)
        {
            var idEl = receiver.Element(CacNs + "PartyIdentification")?.Element(CbcNs + "ID");
            retention.Proveedor = new Client
            {
                NumDoc = idEl?.Value ?? "",
                TipoDoc = ParseDocumentType(idEl?.Attribute("schemeID")?.Value),
                RznSocial = receiver.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value
                    ?? receiver.Descendants(CbcNs + "Name").FirstOrDefault()?.Value ?? ""
            };
        }

        retention.Details = root.Elements(SacNs + "SUNATRetentionDocumentReference")
            .Select((el, i) =>
            {
                var idEl = el.Element(CbcNs + "ID");
                var info = el.Element(SacNs + "SUNATRetentionInformation");
                return new RetentionDetail
                {
                    Orden = i + 1,
                    TipoDoc = idEl?.Attribute("schemeID")?.Value ?? "",
                    NumDoc = idEl?.Value ?? "",
                    FechaEmision = ParseDate(GetValue(el, CbcNs + "IssueDate")),
                    ImpTotal = ParseDecimal(el.Element(CbcNs + "TotalInvoiceAmount")?.Value),
                    CodMoneda = el.Element(CbcNs + "TotalInvoiceAmount")?.Attribute("currencyID")?.Value ?? "PEN",
                    ImpPagar = ParseDecimalNullable(info?.Element(SacNs + "SUNATNetTotalPaid")?.Value)
                };
            }).ToList();

        return retention;
    }

    /// <summary>
    /// Parsea un XML de resumen diario SUNAT.
    /// </summary>
    public static Summary? ParseSummary(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var summary = new Summary
        {
            Correlativo = GetValue(root, CbcNs + "ID") ?? "",
            FechaGeneracion = ParseDate(GetValue(root, CbcNs + "ReferenceDate")),
            FechaEnvio = ParseDate(GetValue(root, CbcNs + "IssueDate"))
        };

        // Company
        var supplier = root.Element(CacNs + "AccountingSupplierParty");
        if (supplier != null)
        {
            summary.Company = new Company
            {
                Ruc = supplier.Element(CbcNs + "CustomerAssignedAccountID")?.Value ?? "",
                RazonSocial = supplier.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        // Details (SummaryDocumentsLine uses sac namespace in greenter, but Khipu uses sac too)
        summary.Details = root.Descendants()
            .Where(e => e.Name.LocalName == "SummaryDocumentsLine")
            .Select((el, i) =>
            {
                var detail = new SummaryDetail
                {
                    Orden = i + 1,
                    SerieNro = GetValue(el, CbcNs + "ID") ?? "",
                    Estado = el.Descendants().FirstOrDefault(e => e.Name.LocalName == "ConditionCode")?.Value ?? "1",
                    Total = ParseDecimal(el.Descendants().FirstOrDefault(e => e.Name.LocalName == "TotalAmount")?.Value)
                };

                var docTypeCode = GetValue(el, CbcNs + "DocumentTypeCode");
                if (int.TryParse(docTypeCode, out var dtc) && Enum.IsDefined(typeof(VoucherType), dtc))
                    detail.TipoDoc = (VoucherType)dtc;

                return detail;
            }).ToList();

        return summary;
    }

    /// <summary>
    /// Parsea un XML de comunicación de baja SUNAT.
    /// </summary>
    public static Voided? ParseVoided(string xml)
    {
        var doc = TryParse(xml);
        if (doc?.Root == null) return null;
        var root = doc.Root;

        var voided = new Voided
        {
            Correlativo = GetValue(root, CbcNs + "ID") ?? "",
            FechaGeneracion = ParseDate(GetValue(root, CbcNs + "ReferenceDate")),
            FechaEnvio = ParseDate(GetValue(root, CbcNs + "IssueDate"))
        };

        var supplier = root.Element(CacNs + "AccountingSupplierParty");
        if (supplier != null)
        {
            voided.Company = new Company
            {
                Ruc = supplier.Element(CbcNs + "CustomerAssignedAccountID")?.Value ?? "",
                RazonSocial = supplier.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
            };
        }

        voided.Details = root.Descendants()
            .Where(e => e.Name.LocalName == "VoidedDocumentsLine")
            .Select((el, i) =>
            {
                return new VoidedDetail
                {
                    Orden = i + 1,
                    TipoDoc = GetValue(el, CbcNs + "DocumentTypeCode") ?? "",
                    SerieNro = $"{el.Descendants().FirstOrDefault(e => e.Name.LocalName == "DocumentSerialID")?.Value}-{el.Descendants().FirstOrDefault(e => e.Name.LocalName == "DocumentNumberID")?.Value}",
                    MotivoBaja = el.Descendants().FirstOrDefault(e => e.Name.LocalName == "VoidReasonDescription")?.Value ?? ""
                };
            }).ToList();

        return voided;
    }

    // ===== Common Parsers =====

    private static Company ParseSupplierParty(XElement root)
    {
        var party = root.Element(CacNs + "AccountingSupplierParty")?.Element(CacNs + "Party");
        if (party == null) return new Company();

        var company = new Company
        {
            Ruc = party.Element(CacNs + "PartyIdentification")?.Element(CbcNs + "ID")?.Value ?? "",
            NombreComercial = party.Element(CacNs + "PartyName")?.Element(CbcNs + "Name")?.Value ?? "",
            RazonSocial = party.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
        };

        var addr = party.Descendants(CacNs + "RegistrationAddress").FirstOrDefault();
        if (addr != null) company.Address = ParseAddress(addr);

        return company;
    }

    private static Client ParseCustomerParty(XElement root)
    {
        var party = root.Element(CacNs + "AccountingCustomerParty")?.Element(CacNs + "Party");
        if (party == null) return new Client();

        var idEl = party.Element(CacNs + "PartyIdentification")?.Element(CbcNs + "ID");
        return new Client
        {
            NumDoc = idEl?.Value ?? "",
            TipoDoc = ParseDocumentType(idEl?.Attribute("schemeID")?.Value),
            RznSocial = party.Descendants(CbcNs + "RegistrationName").FirstOrDefault()?.Value ?? ""
        };
    }

    private static Address ParseAddress(XElement el)
    {
        return new Address
        {
            Ubigeo = GetValue(el, CbcNs + "ID") ?? "",
            CodigoLocal = GetValue(el, CbcNs + "AddressTypeCode") ?? "",
            Urbanizacion = GetValue(el, CbcNs + "CitySubdivisionName") ?? "",
            Provincia = GetValue(el, CbcNs + "CityName") ?? "",
            Departamento = GetValue(el, CbcNs + "CountrySubentity") ?? "",
            Distrito = GetValue(el, CbcNs + "District") ?? "",
            Direccion = el.Descendants(CbcNs + "Line").FirstOrDefault()?.Value ?? ""
        };
    }

    private static void ParseTaxTotals(XElement root, BaseSale sale)
    {
        var taxTotal = root.Element(CacNs + "TaxTotal");
        if (taxTotal == null) return;

        sale.TotalImpuestos = ParseDecimal(taxTotal.Element(CbcNs + "TaxAmount")?.Value);

        foreach (var subtotal in taxTotal.Elements(CacNs + "TaxSubtotal"))
        {
            var scheme = subtotal.Element(CacNs + "TaxCategory")?.Element(CacNs + "TaxScheme");
            var schemeId = scheme?.Element(CbcNs + "ID")?.Value;
            var taxable = ParseDecimal(subtotal.Element(CbcNs + "TaxableAmount")?.Value);
            var amount = ParseDecimal(subtotal.Element(CbcNs + "TaxAmount")?.Value);

            switch (schemeId)
            {
                case "1000": // IGV
                    sale.MtoOperGravadas = taxable;
                    sale.MtoIGV = amount;
                    break;
                case "2000": // ISC
                    sale.MtoBaseIsc = taxable;
                    sale.MtoISC = amount;
                    break;
                case "1016": // IVAP
                    sale.MtoBaseIvap = taxable;
                    sale.MtoIvap = amount;
                    break;
                case "9997": // EXO
                    sale.MtoOperExoneradas = taxable;
                    break;
                case "9998": // INA
                    sale.MtoOperInafectas = taxable;
                    break;
                case "9996": // GRA
                    sale.MtoOperGratuitas = taxable;
                    sale.MtoIGVGratuitas = amount;
                    break;
                case "9995": // EXP
                    sale.MtoOperExportacion = taxable;
                    break;
                case "9999": // OTROS
                    sale.MtoBaseOth = taxable;
                    sale.MtoOtrosTributos = amount;
                    break;
                case "7152": // ICBPER
                    sale.Icbper = amount;
                    break;
            }
        }
    }

    private static SaleDetail ParseSaleDetail(XElement line)
    {
        var qty = line.Elements().FirstOrDefault(e =>
            e.Name.LocalName is "InvoicedQuantity" or "CreditedQuantity" or "DebitedQuantity");
        var item = line.Element(CacNs + "Item");

        var detail = new SaleDetail
        {
            Cantidad = ParseDecimal(qty?.Value),
            Unidad = qty?.Attribute("unitCode")?.Value ?? "NIU",
            MtoValorVenta = ParseDecimal(line.Element(CbcNs + "LineExtensionAmount")?.Value),
            Descripcion = item?.Element(CbcNs + "Description")?.Value ?? "",
            Codigo = item?.Element(CacNs + "SellersItemIdentification")?.Element(CbcNs + "ID")?.Value ?? "",
            CodProdSunat = item?.Element(CacNs + "CommodityClassification")?.Element(CbcNs + "ItemClassificationCode")?.Value ?? ""
        };

        // Price
        var price = line.Element(CacNs + "Price");
        if (price != null)
            detail.MtoValorUnitario = ParseDecimal(price.Element(CbcNs + "PriceAmount")?.Value);

        // PricingReference
        var pricingRef = line.Element(CacNs + "PricingReference")
            ?.Element(CacNs + "AlternativeConditionPrice");
        if (pricingRef != null)
            detail.PrecioVenta = ParseDecimal(pricingRef.Element(CbcNs + "PriceAmount")?.Value);

        // Tax
        var taxSubtotal = line.Element(CacNs + "TaxTotal")?.Element(CacNs + "TaxSubtotal");
        if (taxSubtotal != null)
        {
            var category = taxSubtotal.Element(CacNs + "TaxCategory");
            var afectCode = category?.Element(CbcNs + "TaxExemptionReasonCode")?.Value;
            if (int.TryParse(afectCode, out var afect) && Enum.IsDefined(typeof(TaxType), afect))
                detail.TipoAfectacionIgv = (TaxType)afect;

            detail.Igv = ParseDecimalNullable(taxSubtotal.Element(CbcNs + "TaxAmount")?.Value);
        }

        return detail;
    }

    // ===== Utilities =====

    private static XDocument? TryParse(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return null;
        try { return XDocument.Parse(xml); }
        catch { return null; }
    }

    private static string? GetValue(XElement parent, XName name)
        => parent.Element(name)?.Value;

    private static (string serie, string correlativo) SplitId(string? id)
    {
        if (string.IsNullOrEmpty(id)) return ("", "");
        var parts = id.Split('-', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (id, "");
    }

    private static DateTime ParseDate(string? value)
        => DateTime.TryParse(value, out var d) ? d : default;

    private static decimal ParseDecimal(string? value)
        => decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0m;

    private static decimal? ParseDecimalNullable(string? value)
        => decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;

    private static Currency ParseCurrency(string? code) => code switch
    {
        "PEN" => Currency.Pen,
        "USD" => Currency.Usd,
        "EUR" => Currency.Eur,
        _ => Currency.Pen
    };

    private static DocumentType ParseDocumentType(string? schemeId) => schemeId switch
    {
        "6" => DocumentType.Ruc,
        "1" => DocumentType.Dni,
        _ => DocumentType.Dni
    };
}
