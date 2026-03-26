namespace Khipu.Data.Documents;

/// <summary>
/// Nota de Crédito Electrónica
/// </summary>
public class CreditNote : BaseSale
{
    public CreditNote()
    {
        TipoDoc = Enums.VoucherType.NotaCredito;
    }
    
    public string TipDocAfectado { get; set; } = string.Empty;
    public string NumDocfectado { get; set; } = string.Empty;
    public string CodMotivo { get; set; } = string.Empty; // Catálogo 09
    public string DesMotivo { get; set; } = string.Empty;
}
