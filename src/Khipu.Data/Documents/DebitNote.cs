namespace Khipu.Data.Documents;

/// <summary>
/// Nota de Débito Electrónica
/// </summary>
public class DebitNote : BaseSale
{
    public DebitNote()
    {
        TipoDoc = Enums.VoucherType.NotaDebito;
    }
    
    public string TipDocAfectado { get; set; } = string.Empty;
    public string NumDocfectado { get; set; } = string.Empty;
    public string CodMotivo { get; set; } = string.Empty; // Catálogo 09
    public string DesMotivo { get; set; } = string.Empty;
}
