namespace Khipu.Data.Documents;

using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Common;

/// <summary>
/// Clase base para comprobantes de venta
/// </summary>
public abstract class BaseSale
{
    public Company Company { get; set; } = new();
    public Client Client { get; set; } = new();
    public VoucherType TipoDoc { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }
    public DateTime FechaEmision { get; set; }
    public Currency Moneda { get; set; } = Currency.Pen;
    public List<SaleDetail> Details { get; set; } = new();
    public List<Legend>? Leyendas { get; set; }
    
    // Totales
    public decimal MtoOperGravadas { get; set; }
    public decimal MtoOperExoneradas { get; set; }
    public decimal MtoOperInafectas { get; set; }
    public decimal MtoOperExportacion { get; set; }
    public decimal MtoIGV { get; set; }
    public decimal MtoISC { get; set; }
    public decimal MtoOtrosTributos { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal Redondeo { get; set; }
    public decimal MtoImpVenta { get; set; }
}
