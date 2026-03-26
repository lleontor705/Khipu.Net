namespace Khipu.Data.Common;

/// <summary>
/// Transportista
/// </summary>
public class Transportist
{
    public string TipoDoc { get; set; } = string.Empty;
    public string NumDoc { get; set; } = string.Empty;
    public string RznSocial { get; set; } = string.Empty;
    public string? Placa { get; set; }
}

/// <summary>
/// Vehículo de transporte
/// </summary>
public class Vehicle
{
    public string Placa { get; set; } = string.Empty;
    public string? NroCirculacion { get; set; }
    public string? CodEmisor { get; set; }
    public string? NroAutorizacion { get; set; }
    public string? NroPlacaRemolque { get; set; }
}

/// <summary>
/// Conductor
/// </summary>
public class Driver
{
    public string TipoDoc { get; set; } = string.Empty;
    public string NumDoc { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Licencia { get; set; }
}
