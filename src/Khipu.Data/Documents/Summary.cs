namespace Khipu.Data.Documents;

using Khipu.Data.Common;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Resumen de Boletas - Paridad 100% Greenter Summary
/// </summary>
public class Summary
{
    public Company Company { get; set; } = new();
    public string Correlativo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public DateTime FechaEnvio { get; set; }

    /// <summary>
    /// Moneda (Greenter: moneda)
    /// </summary>
    public string? Moneda { get; set; }

    public List<SummaryDetail> Details { get; set; } = new();
}

/// <summary>
/// Detalle del resumen - Paridad 100% Greenter SummaryDetail
/// </summary>
public class SummaryDetail
{
    public int Orden { get; set; }
    public VoucherType TipoDoc { get; set; }
    public string SerieNro { get; set; } = string.Empty;
    public string ClienteTipoDoc { get; set; } = string.Empty;
    public string ClienteNroDoc { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public DateTime FechaDoc { get; set; }

    /// <summary>
    /// Estado/Condición (Greenter: estado - 1=Adicionar, 2=Modificar, 3=Anular)
    /// </summary>
    public string Estado { get; set; } = "1";

    /// <summary>
    /// Total del documento (Greenter: total)
    /// </summary>
    public decimal Total { get; set; }

    // --- Operaciones ---
    public decimal MtoOperGravadas { get; set; }
    public decimal MtoOperExoneradas { get; set; }
    public decimal MtoOperInafectas { get; set; }
    public decimal MtoOperExportacion { get; set; }

    /// <summary>
    /// Operaciones gratuitas (Greenter: mtoOperGratuitas)
    /// </summary>
    public decimal MtoOperGratuitas { get; set; }

    // --- Impuestos ---
    public decimal MtoIGV { get; set; }

    /// <summary>
    /// Porcentaje IGV para Summary (Greenter: porcentajeIgv)
    /// </summary>
    public decimal? PorcentajeIgv { get; set; }

    public decimal MtoISC { get; set; }
    public decimal MtoOtrosTributos { get; set; }

    /// <summary>
    /// IVAP (Greenter: mtoIvap)
    /// </summary>
    public decimal MtoIvap { get; set; }

    /// <summary>
    /// ICBPER (Greenter: mtoIcbper)
    /// </summary>
    public decimal MtoIcbper { get; set; }

    /// <summary>
    /// Otros cargos (Greenter: mtoOtrosCargos)
    /// </summary>
    public decimal MtoOtrosCargos { get; set; }

    public decimal MtoImpVenta { get; set; }
    public string? CodMoneda { get; set; }

    /// <summary>
    /// Documento de referencia (Greenter: docReferencia)
    /// </summary>
    public Document? DocReferencia { get; set; }

    /// <summary>
    /// Percepción (Greenter: percepcion)
    /// </summary>
    public SummaryPerception? Percepcion { get; set; }
}

/// <summary>
/// Percepción en Resumen - Paridad Greenter
/// </summary>
public class SummaryPerception
{
    public string? CodReg { get; set; }
    public decimal Tasa { get; set; }
    public decimal Mto { get; set; }
    public decimal MtoTotal { get; set; }
    public decimal MtoBase { get; set; }
}
