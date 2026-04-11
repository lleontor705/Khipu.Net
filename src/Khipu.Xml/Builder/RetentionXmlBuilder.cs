namespace Khipu.Xml.Builder;

using System.Globalization;
using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML para Comprobante de Retención - Paridad Greenter retention.xml.twig
/// Namespace SUNAT: urn:sunat:names:specification:ubl:peru:schema:xsd:Retention-1
/// </summary>
public class RetentionXmlBuilder : IXmlBuilder<Retention>
{
    private static readonly XNamespace RetentionNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:Retention-1";
    private static readonly XNamespace SacNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Retention retention)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreateRetentionElement(retention));
        return doc.ToString();
    }

    public string GetFileName(Retention retention)
    {
        return $"{retention.Company.Ruc}-20-{retention.Serie}-{retention.Correlativo:00000000}.xml";
    }

    private XElement CreateRetentionElement(Retention retention)
    {
        var currency = GetCurrencyCode(retention.Moneda);

        return new XElement(RetentionNs + "Retention",
            new XAttribute(XNamespace.Xmlns + "sac", SacNs),
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            // UBLExtensions
            new XElement(ExtNs + "UBLExtensions",
                new XElement(ExtNs + "UBLExtension",
                    new XElement(ExtNs + "ExtensionContent"))),
            new XElement(CbcNs + "UBLVersionID", "2.0"),
            new XElement(CbcNs + "CustomizationID", "1.0"),
            // Signature
            CreateSignature(retention.Company),
            new XElement(CbcNs + "ID", $"{retention.Serie}-{retention.Correlativo}"),
            new XElement(CbcNs + "IssueDate", retention.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", retention.FechaEmision.ToString("HH:mm:ss")),
            // AgentParty (Emisor)
            CreateAgentParty(retention.Company),
            // ReceiverParty (Proveedor)
            CreateReceiverParty(retention.Proveedor),
            // Régimen y tasa de retención - Catálogo 23
            new XElement(SacNs + "SUNATRetentionSystemCode", retention.Regimen ?? "01"),
            new XElement(SacNs + "SUNATRetentionPercent", FormatAmount(retention.Tasa ?? 3.00m)),
            // Totales
            new XElement(CbcNs + "TotalInvoiceAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(retention.MtoRetencion)),
            new XElement(SacNs + "SUNATTotalPaid",
                new XAttribute("currencyID", currency),
                FormatAmount(retention.MtoTotal)),
            // Detalles
            retention.Details.Select(CreateRetentionDocumentReference)
        );
    }

    private XElement CreateSignature(Company company)
    {
        return new XElement(CacNs + "Signature",
            new XElement(CbcNs + "ID", $"SIGN{company.Ruc}"),
            new XElement(CacNs + "SignatoryParty",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID", company.Ruc)),
                new XElement(CacNs + "PartyName",
                    new XElement(CbcNs + "Name", new XCData(company.RazonSocial)))),
            new XElement(CacNs + "DigitalSignatureAttachment",
                new XElement(CacNs + "ExternalReference",
                    new XElement(CbcNs + "URI", "#GREENTER-SIGN"))));
    }

    private XElement CreateAgentParty(Company company)
    {
        return new XElement(CacNs + "AgentParty",
            new XElement(CacNs + "PartyIdentification",
                new XElement(CbcNs + "ID",
                    new XAttribute("schemeID", "6"),
                    company.Ruc)),
            new XElement(CacNs + "PartyName",
                new XElement(CbcNs + "Name", new XCData(company.RazonSocial))),
            new XElement(CacNs + "PostalAddress",
                new XElement(CbcNs + "ID", company.Address.Ubigeo),
                new XElement(CacNs + "AddressLine",
                    new XElement(CbcNs + "Line", new XCData(company.Address.Direccion))),
                new XElement(CacNs + "Country",
                    new XElement(CbcNs + "IdentificationCode", "PE"))),
            new XElement(CacNs + "PartyLegalEntity",
                new XElement(CbcNs + "RegistrationName", new XCData(company.RazonSocial))));
    }

    private XElement CreateReceiverParty(Client proveedor)
    {
        return new XElement(CacNs + "ReceiverParty",
            new XElement(CacNs + "PartyIdentification",
                new XElement(CbcNs + "ID",
                    new XAttribute("schemeID", ((int)proveedor.TipoDoc).ToString()),
                    proveedor.NumDoc)),
            new XElement(CacNs + "PartyName",
                new XElement(CbcNs + "Name", new XCData(proveedor.RznSocial))),
            new XElement(CacNs + "PartyLegalEntity",
                new XElement(CbcNs + "RegistrationName", new XCData(proveedor.RznSocial))));
    }

    private XElement CreateRetentionDocumentReference(RetentionDetail detail)
    {
        var elements = new List<object?>
        {
            new XElement(CbcNs + "ID",
                new XAttribute("schemeID", detail.TipoDoc),
                detail.NumDoc),
            new XElement(CbcNs + "IssueDate", detail.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "TotalInvoiceAmount",
                new XAttribute("currencyID", detail.CodMoneda),
                FormatAmount(detail.ImpTotal))
        };

        // Pagos
        if (detail.Pagos != null)
        {
            foreach (var (pago, index) in detail.Pagos.Select((p, i) => (p, i)))
            {
                elements.Add(new XElement(CacNs + "Payment",
                    new XElement(CbcNs + "ID", index + 1),
                    new XElement(CbcNs + "PaidAmount",
                        new XAttribute("currencyID", detail.CodMoneda),
                        FormatAmount(pago.Monto)),
                    detail.FechaPago.HasValue ?
                        new XElement(CbcNs + "PaidDate", detail.FechaPago.Value.ToString("yyyy-MM-dd")) : null));
            }
        }

        // Información de retención
        if (detail.ImpPagar.HasValue)
        {
            var retencionMonto = detail.ImpTotal - detail.ImpPagar.Value;
            elements.Add(new XElement(SacNs + "SUNATRetentionInformation",
                new XElement(SacNs + "SUNATRetentionAmount",
                    new XAttribute("currencyID", detail.CodMoneda),
                    FormatAmount(retencionMonto)),
                detail.FechaPago.HasValue ?
                    new XElement(SacNs + "SUNATRetentionDate", detail.FechaPago.Value.ToString("yyyy-MM-dd")) : null,
                new XElement(SacNs + "SUNATNetTotalPaid",
                    new XAttribute("currencyID", detail.CodMoneda),
                    FormatAmount(detail.ImpPagar.Value))));
        }

        return new XElement(SacNs + "SUNATRetentionDocumentReference", elements);
    }

    private static string FormatAmount(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    private static string GetCurrencyCode(Data.Enums.Currency currency) => currency switch
    {
        Data.Enums.Currency.Pen => "PEN",
        Data.Enums.Currency.Usd => "USD",
        Data.Enums.Currency.Eur => "EUR",
        _ => "PEN"
    };
}
