namespace Khipu.Data.Documents;

using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Common;

/// <summary>
/// Guía de Remisión Electrónica
/// </summary>
public class Despatch
{
    public Company Company { get; set; } = new();
    public Client Destinatario { get; set; } = new();
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }
    public DateTime FechaEmision { get; set; }
    public string CodMotivoTraslado { get; set; } = string.Empty; // Catálogo 20
    public string DesMotivoTraslado { get; set; } = string.Empty;
    public string? IndTransbordo { get; set; }
    public decimal? PesoTotal { get; set; }
    public string? UndPesoTotal { get; set; }
    public int? NumBultos { get; set; }
    public Address PuntoPartida { get; set; } = new();
    public Address PuntoLlegada { get; set; } = new();
    public List<DespatchDetail> Details { get; set; } = new();
    public Transportist? Transportista { get; set; }
    public Vehicle? Vehiculo { get; set; }
    public List<Driver>? Conductores { get; set; }
}
