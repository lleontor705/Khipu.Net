namespace Khipu.Data.Documents;

using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Resumen de Boletas (Comunicación de Bajas)
/// </summary>
public class Summary
{
    public Company Company { get; set; } = new();
    public string Correlativo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public DateTime FechaEnvio { get; set; }
    public List<SummaryDetail> Details { get; set; } = new();
}

/// <summary>
/// Detalle del resumen
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
    public decimal MtoOperGravadas { get; set; }
    public decimal MtoOperExoneradas { get; set; }
    public decimal MtoOperInafectas { get; set; }
    public decimal MtoOperExportacion { get; set; }
    public decimal MtoIGV { get; set; }
    public decimal MtoISC { get; set; }
    public decimal MtoOtrosTributos { get; set; }
    public decimal MtoImpVenta { get; set; }
    public string? CodMoneda { get; set; }
}
