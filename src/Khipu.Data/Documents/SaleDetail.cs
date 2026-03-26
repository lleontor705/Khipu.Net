namespace Khipu.Data.Documents;

using Khipu.Data.Entities;
using Khipu.Data.Common;

/// <summary>
/// Detalle del comprobante de venta
/// </summary>
public class SaleDetail
{
    public int Orden { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = "NIU"; // Catálogo 03
    public decimal Cantidad { get; set; }
    public decimal MtoValorUnitario { get; set; }
    public decimal MtoValorVenta { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal? Descuento { get; set; }
    public List<Legend>? Leyendas { get; set; }
}
