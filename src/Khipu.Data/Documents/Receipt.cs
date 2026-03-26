namespace Khipu.Data.Documents;

/// <summary>
/// Boleta de Venta Electrónica
/// </summary>
public class Receipt : BaseSale
{
    public Receipt()
    {
        TipoDoc = Enums.VoucherType.Boleta;
    }
    
    public decimal? MtoDescuentos { get; set; }
}
