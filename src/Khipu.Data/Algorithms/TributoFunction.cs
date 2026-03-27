namespace Khipu.Data.Algorithms;

using Khipu.Data.Enums;

/// <summary>
/// Mapeo de tributos SUNAT - Paridad 100% con Greenter TributoFunction
/// </summary>
public static class TributoFunction
{
    public record TributoInfo(string Id, string Code, string Name);

    private static readonly Dictionary<string, TributoInfo> Tributos = new()
    {
        ["1000"] = new("1000", "VAT", "IGV"),
        ["1016"] = new("1016", "VAT", "IVAP"),
        ["2000"] = new("2000", "EXC", "ISC"),
        ["9995"] = new("9995", "FRE", "EXP"),
        ["9996"] = new("9996", "FRE", "GRA"),
        ["9997"] = new("9997", "VAT", "EXO"),
        ["9998"] = new("9998", "FRE", "INA"),
        ["9999"] = new("9999", "OTH", "OTROS"),
    };

    public static TributoInfo? GetByTributo(string? code)
    {
        if (code is not null && Tributos.TryGetValue(code, out var info))
            return info;
        return null;
    }

    public static TributoInfo? GetByAfectacion(TaxType afectacion)
    {
        var code = GetCode(afectacion);
        return GetByTributo(code);
    }

    /// <summary>
    /// Obtiene el código de tributo según el tipo de afectación IGV
    /// Paridad exacta con Greenter getCode()
    /// </summary>
    public static string GetCode(TaxType afectacion)
    {
        return (int)afectacion switch
        {
            10 => "1000", // Gravado → IGV
            17 => "1016", // IVAP
            20 => "9997", // Exonerado
            30 => "9998", // Inafecto
            40 => "9995", // Exportación
            _ => "9996",  // Gratuito (default)
        };
    }
}
