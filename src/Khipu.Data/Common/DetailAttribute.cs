namespace Khipu.Data.Common;

/// <summary>
/// Atributo adicional de línea de detalle - Paridad Greenter DetailAttribute
/// Usado en SaleDetail y DespatchDetail para atributos de producto.
/// </summary>
public class DetailAttribute
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime? FecInicio { get; set; }
    public DateTime? FecFin { get; set; }
    public int? Duracion { get; set; }
}
