namespace Khipu.Data.Documents;

using Khipu.Data.Common;

/// <summary>
/// Factura Electrónica
/// </summary>
public class Invoice : BaseSale
{
    public Invoice()
    {
        TipoDoc = Enums.VoucherType.Factura;
    }
    
    public string? TipoOperacion { get; set; }
    public DateTime? FecVencimiento { get; set; }
    public decimal? MtoDescuentos { get; set; }
    public decimal? MtoCargos { get; set; }
    public decimal? TotalAnticipos { get; set; }
    public Detraction? Detraccion { get; set; }
    public List<Prepayment>? Anticipos { get; set; }
}
