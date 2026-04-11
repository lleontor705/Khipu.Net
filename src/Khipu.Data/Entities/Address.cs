namespace Khipu.Data.Entities;

using Khipu.Data.Enums;

/// <summary>
/// Dirección
/// </summary>
public class Address
{
    public string Ubigeo { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string Urbanizacion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string CodigoLocal { get; set; } = "0000";
    /// <summary>Código de país (default: PE) - Paridad Greenter Address</summary>
    public string CodigoPais { get; set; } = "PE";
}
