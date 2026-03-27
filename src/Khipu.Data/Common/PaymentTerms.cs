namespace Khipu.Data.Common;

/// <summary>
/// Forma de pago - Paridad con Greenter PaymentTerms
/// </summary>
public class PaymentTerms
{
    /// <summary>
    /// Moneda
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Tipo de pago (Contado/Credito)
    /// </summary>
    public string? Tipo { get; set; }

    /// <summary>
    /// Monto
    /// </summary>
    public decimal? Monto { get; set; }
}
