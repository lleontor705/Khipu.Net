namespace Khipu.Xml.Builder;

using System.Globalization;
using System.Xml.Linq;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Xml.Interfaces;

/// <summary>
/// Generador de XML UBL 2.1 para Guía de Remisión Electrónica - Paridad Greenter despatch2022.xml.twig
/// </summary>
public class DespatchXmlBuilder : IXmlBuilder<Despatch>
{
    private static readonly XNamespace DespatchNs = "urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace DsNs = "http://www.w3.org/2000/09/xmldsig#";

    public string Build(Despatch despatch)
    {
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), CreateDespatchElement(despatch));
        return doc.ToString();
    }

    public string GetFileName(Despatch despatch)
    {
        return $"{despatch.Company.Ruc}-09-{despatch.Serie}-{despatch.Correlativo:00000000}.xml";
    }

    private XElement CreateDespatchElement(Despatch despatch)
    {
        return new XElement(DespatchNs + "DespatchAdvice",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs),
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            // UBLExtensions
            new XElement(ExtNs + "UBLExtensions",
                new XElement(ExtNs + "UBLExtension",
                    new XElement(ExtNs + "ExtensionContent"))),
            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "2.0"),
            new XElement(CbcNs + "ID", $"{despatch.Serie}-{despatch.Correlativo}"),
            new XElement(CbcNs + "IssueDate", despatch.FechaEmision.ToString("yyyy-MM-dd")),
            new XElement(CbcNs + "IssueTime", despatch.FechaEmision.ToString("HH:mm:ss")),
            new XElement(CbcNs + "DespatchAdviceTypeCode",
                new XAttribute("listAgencyName", "PE:SUNAT"),
                new XAttribute("listName", "Tipo de Documento"),
                new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01"),
                "09"),
            // Observación
            !string.IsNullOrEmpty(despatch.Observacion ?? despatch.DesMotivoTraslado) ?
                new XElement(CbcNs + "Note", new XCData(despatch.Observacion ?? despatch.DesMotivoTraslado)) : null,
            // AdditionalDocumentReference (Greenter: addDocs)
            despatch.DocumentosAdicionales?.Select(ad => new XElement(CacNs + "AdditionalDocumentReference",
                new XElement(CbcNs + "ID", ad.Nro),
                new XElement(CbcNs + "DocumentTypeCode", ad.Tipo),
                !string.IsNullOrEmpty(ad.Emisor) ?
                    new XElement(CbcNs + "DocumentDescription", ad.Emisor) : null)),
            // Signature
            CreateSignature(despatch.Company),
            // DespatchSupplierParty (Emisor)
            CreateDespatchSupplierParty(despatch.Company),
            // DeliveryCustomerParty (Destinatario)
            CreateDeliveryCustomerParty(despatch.Destinatario),
            // SellerSupplierParty (Tercero proveedor)
            despatch.Tercero != null ? CreatePartyNode("SellerSupplierParty", despatch.Tercero) : null,
            // BuyerCustomerParty (Comprador)
            despatch.Comprador != null ? CreatePartyNode("BuyerCustomerParty", despatch.Comprador) : null,
            // Shipment
            CreateShipment(despatch),
            // DespatchLines
            despatch.Details.Select((d, i) => CreateDespatchLine(d, i + 1))
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

    private XElement CreateDespatchSupplierParty(Company company)
    {
        return new XElement(CacNs + "DespatchSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", "6"),
                        company.Ruc)),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(company.RazonSocial)))));
    }

    private XElement CreateDeliveryCustomerParty(Client client)
    {
        return new XElement(CacNs + "DeliveryCustomerParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", ((int)client.TipoDoc).ToString()),
                        client.NumDoc)),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(client.RznSocial)))));
    }

    private XElement CreatePartyNode(string elementName, Client client)
    {
        return new XElement(CacNs + elementName,
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", ((int)client.TipoDoc).ToString()),
                        client.NumDoc)),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(client.RznSocial)))));
    }

    private XElement CreateShipment(Despatch despatch)
    {
        return new XElement(CacNs + "Shipment",
            new XElement(CbcNs + "ID", "SUNAT_Envio"),
            new XElement(CbcNs + "HandlingCode",
                new XAttribute("listAgencyName", "PE:SUNAT"),
                new XAttribute("listName", "Motivo de traslado"),
                new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo20"),
                despatch.CodMotivoTraslado),
            !string.IsNullOrEmpty(despatch.DesMotivoTraslado) ?
                new XElement(CbcNs + "HandlingInstructions", despatch.DesMotivoTraslado) : null,
            // Peso bruto total
            despatch.PesoTotal.HasValue ?
                new XElement(CbcNs + "GrossWeightMeasure",
                    new XAttribute("unitCode", despatch.UndPesoTotal ?? "KGM"),
                    despatch.PesoTotal.Value.ToString("F3", CultureInfo.InvariantCulture)) : null,
            // Número de bultos
            despatch.NumBultos.HasValue ?
                new XElement(CbcNs + "TotalTransportHandlingUnitQuantity", despatch.NumBultos.Value) : null,
            // ShipmentStage - Modo de transporte
            CreateShipmentStage(despatch),
            // Delivery - Dirección de llegada
            CreateDelivery(despatch.PuntoLlegada),
            // TransportHandlingUnit - Vehículo
            despatch.Vehiculo != null ? CreateTransportHandlingUnit(despatch.Vehiculo) : null,
            // Origin - Dirección de partida
            CreateOriginAddress(despatch.PuntoPartida),
            // FirstArrivalPortLocation (si se necesita en el futuro)
            null
        );
    }

    private XElement CreateShipmentStage(Despatch despatch)
    {
        var elements = new List<object?>
        {
            new XElement(CbcNs + "TransportModeCode",
                new XAttribute("listName", "Modalidad de traslado"),
                new XAttribute("listAgencyName", "PE:SUNAT"),
                new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo18"),
                despatch.IndTransbordo ?? "01") // 01=Transporte público, 02=Transporte privado
        };

        // Transportista (solo para transporte público)
        if (despatch.Transportista != null)
        {
            elements.Add(new XElement(CacNs + "CarrierParty",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", despatch.Transportista.TipoDoc),
                        despatch.Transportista.NumDoc)),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", new XCData(despatch.Transportista.RznSocial)))));
        }

        // Conductores
        if (despatch.Conductores != null)
        {
            foreach (var (driver, index) in despatch.Conductores.Select((d, i) => (d, i)))
            {
                elements.Add(new XElement(CacNs + "DriverPerson",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", driver.TipoDoc),
                        driver.NumDoc),
                    new XElement(CbcNs + "FirstName", driver.Nombres),
                    new XElement(CbcNs + "FamilyName", driver.Apellidos),
                    new XElement(CbcNs + "JobTitle", index == 0 ? "Principal" : "Secundario"),
                    !string.IsNullOrEmpty(driver.Licencia) ?
                        new XElement(CacNs + "IdentityDocumentReference",
                            new XElement(CbcNs + "ID", driver.Licencia)) : null));
            }
        }

        return new XElement(CacNs + "ShipmentStage", elements);
    }

    private XElement CreateDelivery(Address llegada)
    {
        return new XElement(CacNs + "Delivery",
            new XElement(CacNs + "DeliveryAddress",
                new XElement(CbcNs + "ID", llegada.Ubigeo),
                new XElement(CacNs + "AddressLine",
                    new XElement(CbcNs + "Line", new XCData(llegada.Direccion))),
                new XElement(CacNs + "Country",
                    new XElement(CbcNs + "IdentificationCode",
                        new XAttribute("listID", "ISO 3166-1"),
                        new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                        new XAttribute("listName", "Country"),
                        "PE"))));
    }

    private XElement CreateTransportHandlingUnit(Vehicle vehiculo)
    {
        var elements = new List<object?>
        {
            new XElement(CbcNs + "ID", vehiculo.Placa),
            new XElement(CacNs + "TransportEquipment",
                new XElement(CbcNs + "ID", vehiculo.Placa))
        };

        // Vehículo secundario (remolque)
        if (!string.IsNullOrEmpty(vehiculo.NroPlacaRemolque))
        {
            elements.Add(new XElement(CacNs + "AttachedTransportEquipment",
                new XElement(CbcNs + "ID", vehiculo.NroPlacaRemolque)));
        }

        return new XElement(CacNs + "TransportHandlingUnit", elements);
    }

    private XElement CreateOriginAddress(Address partida)
    {
        return new XElement(CacNs + "OriginAddress",
            new XElement(CbcNs + "ID", partida.Ubigeo),
            new XElement(CacNs + "AddressLine",
                new XElement(CbcNs + "Line", new XCData(partida.Direccion))),
            new XElement(CacNs + "Country",
                new XElement(CbcNs + "IdentificationCode",
                    new XAttribute("listID", "ISO 3166-1"),
                    new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                    new XAttribute("listName", "Country"),
                    "PE")));
    }

    private XElement CreateDespatchLine(DespatchDetail detail, int lineNumber)
    {
        return new XElement(CacNs + "DespatchLine",
            new XElement(CbcNs + "ID", lineNumber),
            new XElement(CbcNs + "DeliveredQuantity",
                new XAttribute("unitCode", detail.Unidad),
                detail.Cantidad.ToString("F2", CultureInfo.InvariantCulture)),
            new XElement(CacNs + "OrderLineReference",
                new XElement(CbcNs + "LineID", lineNumber)),
            new XElement(CacNs + "Item",
                new XElement(CbcNs + "Description", new XCData(detail.Descripcion)),
                new XElement(CacNs + "SellersItemIdentification",
                    new XElement(CbcNs + "ID", detail.Codigo)),
                !string.IsNullOrEmpty(detail.CodProdSunat) ?
                    new XElement(CacNs + "CommodityClassification",
                        new XElement(CbcNs + "ItemClassificationCode",
                            new XAttribute("listID", "UNSPSC"),
                            new XAttribute("listAgencyName", "GS1 US"),
                            new XAttribute("listName", "Item Classification"),
                            detail.CodProdSunat)) : null));
    }
}
