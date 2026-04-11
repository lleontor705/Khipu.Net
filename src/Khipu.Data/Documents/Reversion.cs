namespace Khipu.Data.Documents;

using Khipu.Data.Entities;

/// <summary>
/// Reversión de documentos - Paridad Greenter Reversion
/// Extiende Voided con prefijo "RR" en lugar de "RA".
/// </summary>
public class Reversion : Voided
{
    /// <summary>
    /// Genera el ID XML con prefijo RR (Reversión) en lugar de RA (Anulación).
    /// Paridad con Greenter Reversion::getXmlId()
    /// </summary>
    public string GetXmlId()
    {
        var fecha = FechaGeneracion.ToString("yyyyMMdd");
        return $"{Company.Ruc}-RR-{fecha}-{Correlativo}";
    }
}
