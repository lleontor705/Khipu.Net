namespace Khipu.Xml.Builder;

using System.Globalization;
using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Enums;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador XML Resumen de Boletas - Paridad 100% Greenter summary.xml.twig
/// </summary>
public class SummaryXmlBuilder : IXmlBuilder<Summary>
{
    private static readonly XNamespace SummaryNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1";
    private static readonly XNamespace SacNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Summary summary)
    {
        EnsureNoDuplicateReferences(summary);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreateSummaryElement(summary));
        return doc.ToString();
    }

    public string GetFileName(Summary summary)
    {
        var fecha = summary.FechaGeneracion.ToString("yyyyMMdd");
        var correlativo = summary.Correlativo.PadLeft(3, '0');
        return $"{summary.Company.Ruc}-RC-{fecha}-{correlativo}.xml";
    }

    private XElement CreateSummaryElement(Summary summary)
    {
        return new XElement(SummaryNs + "SummaryDocuments",
            new XAttribute(XNamespace.Xmlns + "sac", SacNs),
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            new XElement(ExtNs + "UBLExtensions", new XElement(ExtNs + "UBLExtension", new XElement(ExtNs + "ExtensionContent"))),
            new XElement(CbcNs + "UBLVersionID", "2.0"),
            new XElement(CbcNs + "CustomizationID", "1.1"),
            new XElement(CbcNs + "ID", summary.Correlativo.PadLeft(3, '0')),
            new XElement(CbcNs + "ReferenceDate", summary.FechaGeneracion.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueDate", summary.FechaEnvio.ToString("yyyy-MM-dd")),
            // Signature
            new XElement(CacNs + "Signature",
                new XElement(CbcNs + "ID", $"SIGN{summary.Company.Ruc}"),
                new XElement(CacNs + "SignatoryParty",
                    new XElement(CacNs + "PartyIdentification", new XElement(CbcNs + "ID", summary.Company.Ruc)),
                    new XElement(CacNs + "PartyName", new XElement(CbcNs + "Name", new XCData(summary.Company.RazonSocial)))
                ),
                new XElement(CacNs + "DigitalSignatureAttachment",
                    new XElement(CacNs + "ExternalReference", new XElement(CbcNs + "URI", "#GREENTER-SIGN")))
            ),
            new XElement(CacNs + "AccountingSupplierParty",
                new XElement(CbcNs + "CustomerAssignedAccountID", summary.Company.Ruc),
                new XElement(CbcNs + "AdditionalAccountID", "6"),
                new XElement(CacNs + "Party", new XElement(CacNs + "PartyLegalEntity", new XElement(CbcNs + "RegistrationName", new XCData(summary.Company.RazonSocial))))),
            summary.Details.OrderBy(d => d.Orden == 0 ? int.MaxValue : d.Orden).Select((d, i) => CreateSummaryDocument(d, i + 1, summary.Moneda ?? "PEN"))
        );
    }

    private XElement CreateSummaryDocument(SummaryDetail detail, int lineNumber, string defaultCurrency)
    {
        var currency = string.IsNullOrWhiteSpace(detail.CodMoneda) ? defaultCurrency : detail.CodMoneda;
        var totalAmount = detail.Total > 0 ? detail.Total : CalculateSummaryTotal(detail);

        return new XElement(SacNs + "SummaryDocumentsLine",
            new XElement(CbcNs + "LineID", lineNumber),
            new XElement(CbcNs + "DocumentTypeCode", ((int)detail.TipoDoc).ToString().PadLeft(2, '0')),
            new XElement(CbcNs + "ID", detail.SerieNro),
            new XElement(CacNs + "AccountingCustomerParty",
                new XElement(CbcNs + "CustomerAssignedAccountID", detail.ClienteNroDoc),
                new XElement(CbcNs + "AdditionalAccountID", detail.ClienteTipoDoc)),
            // BillingReference (Greenter: docReferencia)
            detail.DocReferencia != null ? new XElement(CacNs + "BillingReference",
                new XElement(CacNs + "InvoiceDocumentReference",
                    new XElement(CbcNs + "ID", detail.DocReferencia.NroDoc),
                    new XElement(CbcNs + "DocumentTypeCode", detail.DocReferencia.TipoDoc)
                )) : null,
            // Percepción (Greenter: percepcion)
            detail.Percepcion != null ? CreateSummaryPerception(detail.Percepcion) : null,
            // Estado (Greenter: estado - default "1")
            new XElement(CacNs + "Status", new XElement(CbcNs + "ConditionCode", detail.Estado)),
            new XElement(SacNs + "TotalAmount", new XAttribute("currencyID", currency), FormatAmount(totalAmount)),
            // BillingPayment (Greenter: sac:BillingPayment con InstructionID 01-05)
            detail.MtoOperGravadas > 0 ? CreateBillingPayment(detail.MtoOperGravadas, "01", currency) : null,
            detail.MtoOperExoneradas > 0 ? CreateBillingPayment(detail.MtoOperExoneradas, "02", currency) : null,
            detail.MtoOperInafectas > 0 ? CreateBillingPayment(detail.MtoOperInafectas, "03", currency) : null,
            detail.MtoOperExportacion > 0 ? CreateBillingPayment(detail.MtoOperExportacion, "04", currency) : null,
            detail.MtoOperGratuitas > 0 ? CreateBillingPayment(detail.MtoOperGratuitas, "05", currency) : null,
            // OtrosCargos AllowanceCharge (Greenter: mtoOtrosCargos)
            detail.MtoOtrosCargos > 0 ? new XElement(CacNs + "AllowanceCharge",
                new XElement(CbcNs + "ChargeIndicator", "true"),
                new XElement(CbcNs + "Amount", new XAttribute("currencyID", currency), FormatAmount(detail.MtoOtrosCargos))
            ) : null,
            // Tax totals - Greenter: IVAP o IGV (mutuamente excluyentes), luego ISC, OTROS, ICBPER
            detail.MtoIvap > 0
                ? CreateSummaryTaxTotal("1016", "IVAP", "VAT", detail.MtoIvap, currency, null)
                : CreateSummaryTaxTotal("1000", "IGV", "VAT", detail.MtoIGV, currency, detail.PorcentajeIgv),
            detail.MtoISC > 0 ? CreateSummaryTaxTotal("2000", "ISC", "EXC", detail.MtoISC, currency, null) : null,
            detail.MtoOtrosTributos > 0 ? CreateSummaryTaxTotal("9999", "OTROS", "OTH", detail.MtoOtrosTributos, currency, null) : null,
            detail.MtoIcbper > 0 ? CreateSummaryTaxTotal("7152", "ICBPER", "OTH", detail.MtoIcbper, currency, null) : null
        );
    }

    /// <summary>
    /// BillingPayment - Greenter: sac:BillingPayment con PaidAmount + InstructionID
    /// </summary>
    private XElement CreateBillingPayment(decimal amount, string instructionId, string currency)
    {
        return new XElement(SacNs + "BillingPayment",
            new XElement(CbcNs + "PaidAmount", new XAttribute("currencyID", currency), FormatAmount(amount)),
            new XElement(CbcNs + "InstructionID", instructionId)
        );
    }

    /// <summary>
    /// Percepción en resumen - Greenter: SUNATPerceptionSummaryDocumentReference
    /// </summary>
    private XElement CreateSummaryPerception(SummaryPerception perc)
    {
        return new XElement(SacNs + "SUNATPerceptionSummaryDocumentReference",
            new XElement(SacNs + "SUNATPerceptionSystemCode", perc.CodReg),
            new XElement(SacNs + "SUNATPerceptionPercent", FormatAmount(perc.Tasa)),
            new XElement(CbcNs + "TotalInvoiceAmount", new XAttribute("currencyID", "PEN"), FormatAmount(perc.Mto)),
            new XElement(SacNs + "SUNATTotalCashed", new XAttribute("currencyID", "PEN"), FormatAmount(perc.MtoTotal)),
            new XElement(CbcNs + "TaxableAmount", new XAttribute("currencyID", "PEN"), FormatAmount(perc.MtoBase))
        );
    }

    private XElement CreateSummaryTaxTotal(string id, string name, string typeCode, decimal amount, string currency, decimal? percent)
    {
        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", currency), FormatAmount(amount)),
            new XElement(CacNs + "TaxSubtotal",
                new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", currency), FormatAmount(amount)),
                new XElement(CacNs + "TaxCategory",
                    percent.HasValue ? new XElement(CbcNs + "Percent", FormatAmount(percent.Value)) : null,
                    new XElement(CacNs + "TaxScheme",
                        new XElement(CbcNs + "ID", id),
                        new XElement(CbcNs + "Name", name),
                        new XElement(CbcNs + "TaxTypeCode", typeCode)))));
    }

    private static void EnsureNoDuplicateReferences(Summary summary)
    {
        var duplicated = summary.Details
            .GroupBy(d => d.SerieNro, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicated is not null)
        {
            throw new InvalidOperationException($"DUPLICATE_REFERENCE:{duplicated.Key}");
        }
    }

    private static decimal CalculateSummaryTotal(SummaryDetail detail)
    {
        if (detail.MtoImpVenta > 0)
        {
            return detail.MtoImpVenta;
        }

        return detail.MtoOperGravadas +
               detail.MtoOperExoneradas +
               detail.MtoOperInafectas +
               detail.MtoOperExportacion +
               detail.MtoIGV +
               detail.MtoISC +
               detail.MtoOtrosTributos;
    }

    private static string FormatAmount(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);
}
