namespace Khipu.Data.Common;

/// <summary>
/// Anticipo/Prepago
/// </summary>
public class Prepayment
{
    public int Nro { get; set; }
    public string TipoDoc { get; set; } = string.Empty;
    public string NroDoc { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
