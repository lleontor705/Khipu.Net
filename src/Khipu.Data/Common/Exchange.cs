namespace Khipu.Data.Common;

/// <summary>
/// Tipo de cambio - Paridad Greenter Exchange
/// Usado en PerceptionDetail y RetentionDetail para conversión de moneda.
/// </summary>
public class Exchange
{
    /// <summary>Moneda de referencia (ej: USD)</summary>
    public string MonedaRef { get; set; } = string.Empty;
    /// <summary>Moneda objetivo (ej: PEN)</summary>
    public string MonedaObj { get; set; } = string.Empty;
    /// <summary>Factor de tipo de cambio</summary>
    public decimal Factor { get; set; }
    /// <summary>Fecha del tipo de cambio</summary>
    public DateTime Fecha { get; set; }
}
