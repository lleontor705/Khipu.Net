namespace Khipu.Data.Entities;

using Khipu.Data.Enums;

/// <summary>
/// Empresa emisora del comprobante
/// </summary>
public class Company
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string? Email { get; set; }
    public string? Telephone { get; set; }
}
