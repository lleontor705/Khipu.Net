namespace Khipu.Data.Documents;

using Khipu.Data.Enums;
using Khipu.Data.Common;

/// <summary>
/// Nota de Débito Electrónica
/// </summary>
public class DebitNote : BaseSale
{
    public DebitNote()
    {
        TipoDoc = VoucherType.NotaDebito;
    }
    
    public string TipDocAfectado { get; set; } = string.Empty;
    public string NumDocAfectado { get; set; } = string.Empty; // CORREGIDO
    public string CodMotivo { get; set; } = string.Empty; // Catálogo 09
    public string DesMotivo { get; set; } = string.Empty;
}
