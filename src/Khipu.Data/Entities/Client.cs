namespace Khipu.Data.Entities;

using Khipu.Data.Enums;

/// <summary>
/// Cliente/Receptor del comprobante
/// </summary>
public class Client
{
    public DocumentType TipoDoc { get; set; }
    public string NumDoc { get; set; } = string.Empty;
    public string RznSocial { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
}
