namespace Khipu.Data.Common;

/// <summary>
/// Cargos y descuentos (Catálogo 53) - Paridad con Greenter Charge
/// </summary>
public class Charge
{
    /// <summary>
    /// Código de tipo de cargo/descuento (Catálogo 53)
    /// </summary>
    public string? CodTipo { get; set; }

    /// <summary>
    /// Factor del cargo/descuento
    /// </summary>
    public decimal? Factor { get; set; }

    /// <summary>
    /// Monto del cargo/descuento
    /// </summary>
    public decimal? Monto { get; set; }

    /// <summary>
    /// Monto base para el cálculo
    /// </summary>
    public decimal? MontoBase { get; set; }
}
