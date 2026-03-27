namespace Khipu.Data.Common;

/// <summary>
/// Cuota de pago a crédito - Paridad con Greenter Cuota
/// </summary>
public class Cuota
{
    /// <summary>
    /// Moneda
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Monto de la cuota
    /// </summary>
    public decimal Monto { get; set; }

    /// <summary>
    /// Fecha de pago
    /// </summary>
    public DateTime FechaPago { get; set; }
}
