namespace Khipu.Data.Documents;

using Khipu.Data.Common;

/// <summary>
/// Detalle de Guía de Remisión
/// </summary>
public class DespatchDetail
{
    public int Orden { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = "NIU";
    public decimal Cantidad { get; set; }
    public string? CodProdSunat { get; set; }
    /// <summary>Atributos adicionales del ítem (Greenter: atributos)</summary>
    public List<DetailAttribute>? Atributos { get; set; }
}
