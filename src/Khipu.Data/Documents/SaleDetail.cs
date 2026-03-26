namespace Khipu.Data.Documents;

using Khipu.Data.Enums;
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
    
    /// <summary>
    /// Tipo de afectación del IGV (Catálogo 07)
    /// </summary>
    public TaxType TipoAfectacionIgv { get; set; } = TaxType.Gravado;
    
    /// <summary>
    /// Tasa de IGV (por defecto 18%)
    /// </summary>
    public decimal TasaIgv { get; set; } = 0.18m;
    
    /// <summary>
    /// Monto de ISC
    /// </summary>
    public decimal? MtoIsc { get; set; }
    
    /// <summary>
    /// Tasa de ISC
    /// </summary>
    public decimal? TasaIsc { get; set; }
    
    /// <summary>
    /// Código de tributo (Catálogo 05)
    /// </summary>
    public string? CodTributo { get; set; }
    
    /// <summary>
    /// Código de producto SUNAT
    /// </summary>
    public string? CodProdSunat { get; set; }
    
    public List<Legend>? Leyendas { get; set; }
}
