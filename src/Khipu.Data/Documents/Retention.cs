namespace Khipu.Data.Documents;

using Khipu.Data.Common;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Comprobante de Retención
/// </summary>
public class Retention
{
    public Company Company { get; set; } = new();
    public Client Proveedor { get; set; } = new();
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }
    public DateTime FechaEmision { get; set; }
    public Currency Moneda { get; set; } = Currency.Pen;
    public List<RetentionDetail> Details { get; set; } = new();
    public decimal MtoRetencion { get; set; }
    public decimal MtoTotal { get; set; }
    /// <summary>Código de régimen de retención (Catálogo 23 SUNAT)</summary>
    public string? Regimen { get; set; }
    /// <summary>Tasa de retención (ej: 3.00 para 3%)</summary>
    public decimal? Tasa { get; set; }
    /// <summary>Observaciones</summary>
    public string? Observacion { get; set; }
}

/// <summary>
/// Detalle de retención
/// </summary>
public class RetentionDetail
{
    public int Orden { get; set; }
    public string TipoDoc { get; set; } = string.Empty;
    public string NumDoc { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaPago { get; set; }
    public decimal ImpTotal { get; set; }
    public decimal? ImpPagar { get; set; }
    public string CodMoneda { get; set; } = "PEN";
    public List<Payment>? Pagos { get; set; }
    /// <summary>Tipo de cambio (si moneda diferente a PEN)</summary>
    public Exchange? TipoCambio { get; set; }
}

/// <summary>
/// Pago
/// </summary>
public class Payment
{
    public string FormaPago { get; set; } = string.Empty; // Catálogo 51
    public decimal Monto { get; set; }
    public string? NumOperacion { get; set; }
}
