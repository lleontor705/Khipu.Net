namespace Khipu.Data.Common;

/// <summary>
/// Detracción del comprobante
/// </summary>
public class Detraction
{
    /// <summary>Monto de detracción (Greenter: mount)</summary>
    public decimal Monto { get; set; }
    public string CtaBanco { get; set; } = string.Empty;
    public string CodBienDetraccion { get; set; } = string.Empty; // Catálogo 54
    public decimal? Porcentaje { get; set; }
    public string? CodMedioPago { get; set; } // Catálogo 59
    /// <summary>Valor referencial de transporte terrestre (Greenter: valueRef)</summary>
    public decimal? ValueRef { get; set; }
}
