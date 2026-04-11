namespace Khipu.Ws.Reader;

using System.Xml.Linq;
using Khipu.Ws.Models;

/// <summary>
/// Parser de CDR (Constancia de Recepción) de SUNAT - Paridad Greenter DomCdrReader
/// Extrae información de respuesta del XML CDR contenido en el ZIP de respuesta.
/// </summary>
public static class CdrReader
{
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

    /// <summary>
    /// Parsea un XML de CDR y extrae la respuesta estructurada.
    /// </summary>
    public static CdrDetail? Parse(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return null;

        try
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            if (root == null) return null;

            var result = new CdrDetail();

            // Extraer notas del documento (cbc:Note directos del root)
            var notes = root.Descendants(CbcNs + "Note")
                .Where(n => n.Parent == root)
                .Select(n => n.Value)
                .ToList();
            result.Notes = notes.Count > 0 ? notes : null;

            // Buscar DocumentResponse
            var docResponse = root.Descendants(CacNs + "DocumentResponse").FirstOrDefault();
            if (docResponse == null) return result;

            // Response dentro de DocumentResponse
            var response = docResponse.Element(CacNs + "Response");
            if (response != null)
            {
                result.Id = response.Element(CbcNs + "ReferenceID")?.Value;
                result.Code = response.Element(CbcNs + "ResponseCode")?.Value;
                result.Description = response.Element(CbcNs + "Description")?.Value;
            }

            // DocumentReference dentro de DocumentResponse
            var docRef = docResponse.Element(CacNs + "DocumentReference");
            if (docRef != null)
            {
                result.Reference = docRef.Element(CbcNs + "DocumentDescription")?.Value;
            }

            // Calcular IsAccepted: código 0 o >= 4000 (observaciones, no rechazos)
            result.IsAccepted = IsResponseAccepted(result.Code);

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parsea CDR desde bytes ZIP (extrae XML del ZIP y luego parsea).
    /// </summary>
    public static CdrDetail? ParseFromZip(byte[]? zipData)
    {
        if (zipData == null || zipData.Length == 0) return null;

        try
        {
            var xml = Khipu.Ws.Helpers.ZipHelper.ExtractXml(zipData);
            return Parse(xml);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Greenter: code == '0' || code >= '4000' → aceptado
    /// Códigos 0 = aceptado, 4000+ = observaciones (aceptado con observaciones)
    /// Códigos 2000-3999 = rechazado
    /// </summary>
    private static bool IsResponseAccepted(string? code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        if (code == "0") return true;
        if (int.TryParse(code, out var numCode))
        {
            return numCode >= 4000;
        }
        return false;
    }
}
