namespace Khipu.Data.Documents;

using Khipu.Data.Entities;

/// <summary>
/// Comunicación de Bajas
/// </summary>
public class Voided
{
    public Company Company { get; set; } = new();
    public string Correlativo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public DateTime FechaEnvio { get; set; }
    public List<VoidedDetail> Details { get; set; } = new();
}

/// <summary>
/// Detalle de baja
/// </summary>
public class VoidedDetail
{
    public int Orden { get; set; }
    public string TipoDoc { get; set; } = string.Empty;
    public string SerieNro { get; set; } = string.Empty;
    public DateTime FechaDoc { get; set; }
    public string MotivoBaja { get; set; } = string.Empty;
}
