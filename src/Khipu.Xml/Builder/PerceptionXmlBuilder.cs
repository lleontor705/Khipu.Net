namespace Khipu.Xml.Builder;

using System.Globalization;
using System.Xml.Linq;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML para Comprobante de Percepción - Paridad Greenter perception.xml.twig
/// Namespace SUNAT: urn:sunat:names:specification:ubl:peru:schema:xsd:Perception-1
/// </summary>
public class PerceptionXmlBuilder : IXmlBuilder<Perception>
{
    private static readonly XNamespace PerceptionNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:Perception-1";
    private static readonly XNamespace SacNs = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Perception perception)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreatePerceptionElement(perception));
        return doc.ToString();
    }

    public string GetFileName(Perception perception)
    {
        return $"{perception.Company.Ruc}-40-{perception.Serie}-{perception.Correlativo:00000000}.xml";
    }

    private XElement CreatePerceptionElement(Perception perception)
    {
        var currency = GetCurrencyCode(perception.Moneda);

        return new XElement(PerceptionNs + "Perception",
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
            CreateSignature(perception.Company),
            new XElement(CbcNs + "ID", $"{perception.Serie}-{perception.Correlativo}"),
            new XElement(CbcNs + "IssueDate", perception.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", perception.FechaEmision.ToString("HH:mm:ss")),
            // AgentParty (Emisor)
            CreateAgentParty(perception.Company),
            // ReceiverParty (Proveedor)
            CreateReceiverParty(perception.Proveedor),
            // Régimen y tasa de percepción
            new XElement(SacNs + "SUNATPerceptionSystemCode", perception.Regimen ?? perception.Details.FirstOrDefault()?.CodReg ?? "01"),
            new XElement(SacNs + "SUNATPerceptionPercent", FormatAmount(perception.Tasa ?? perception.Details.FirstOrDefault()?.Porcentaje ?? 2.00m)),
            // Totales
            new XElement(CbcNs + "TotalInvoiceAmount",
                new XAttribute("currencyID", currency),
                FormatAmount(perception.MtoPercepcion)),
            new XElement(SacNs + "SUNATTotalCashed",
                new XAttribute("currencyID", currency),
                FormatAmount(perception.MtoTotalCobrar)),
            // Detalles
            perception.Details.Select(CreatePerceptionDocumentReference)
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

    private XElement CreatePerceptionDocumentReference(PerceptionDetail detail)
    {
        return new XElement(SacNs + "SUNATPerceptionDocumentReference",
            new XElement(CbcNs + "ID",
                new XAttribute("schemeID", detail.TipoDoc),
                detail.NumDoc),
            new XElement(CbcNs + "IssueDate", detail.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "TotalInvoiceAmount",
                new XAttribute("currencyID", detail.CodMoneda),
                FormatAmount(detail.ImpTotal)),
            // Cobro (pago)
            new XElement(CacNs + "Payment",
                new XElement(CbcNs + "ID", detail.Orden),
                new XElement(CbcNs + "PaidAmount",
                    new XAttribute("currencyID", detail.CodMoneda),
                    FormatAmount(detail.ImpCobrar)),
                new XElement(CbcNs + "PaidDate", detail.FechaEmision.ToString("yyyy-MM-dd"))),
            // Información de percepción
            new XElement(SacNs + "SUNATPerceptionInformation",
                new XElement(SacNs + "SUNATPerceptionAmount",
                    new XAttribute("currencyID", detail.CodMoneda),
                    FormatAmount(detail.Mto)),
                new XElement(SacNs + "SUNATPerceptionDate", detail.FechaEmision.ToString("yyyy-MM-dd")),
                new XElement(SacNs + "SUNATNetTotalCashed",
                    new XAttribute("currencyID", detail.CodMoneda),
                    FormatAmount(detail.ImpCobrar + detail.Mto))));
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
