namespace Khipu.Data.Documents;

using Khipu.Data.Common;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Comprobante de Percepción
/// </summary>
public class Perception
{
    public Company Company { get; set; } = new();
    public Client Proveedor { get; set; } = new();
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }
    public DateTime FechaEmision { get; set; }
    public Currency Moneda { get; set; } = Currency.Pen;
    public List<PerceptionDetail> Details { get; set; } = new();
    public decimal MtoPercepcion { get; set; }
    public decimal MtoTotal { get; set; }
    public decimal MtoTotalCobrar { get; set; }
    /// <summary>Código de régimen de percepción (Catálogo 22 SUNAT)</summary>
    public string? Regimen { get; set; }
    /// <summary>Tasa de percepción (ej: 2.00 para 2%)</summary>
    public decimal? Tasa { get; set; }
    /// <summary>Observaciones</summary>
    public string? Observacion { get; set; }
}

/// <summary>
/// Detalle de percepción
/// </summary>
public class PerceptionDetail
{
    public int Orden { get; set; }
    public string TipoDoc { get; set; } = string.Empty;
    public string NumDoc { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string CodMoneda { get; set; } = "PEN";
    public decimal ImpTotal { get; set; }
    public decimal ImpCobrar { get; set; }
    public string? CodReg { get; set; } // Catálogo 22
    public decimal Porcentaje { get; set; }
    public decimal MtoBase { get; set; }
    public decimal Mto { get; set; }
    /// <summary>Fecha de percepción</summary>
    public DateTime? FechaPercepcion { get; set; }
    /// <summary>Tipo de cambio (si moneda diferente a PEN)</summary>
    public Exchange? TipoCambio { get; set; }
}
