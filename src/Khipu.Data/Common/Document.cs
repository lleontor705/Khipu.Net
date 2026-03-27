namespace Khipu.Data.Common;

/// <summary>
/// Documento relacionado (guías de remisión, etc.) - Paridad con Greenter Document
/// </summary>
public class Document
{
    /// <summary>
    /// Tipo de documento
    /// </summary>
    public string? TipoDoc { get; set; }

    /// <summary>
    /// Número de documento
    /// </summary>
    public string? NroDoc { get; set; }
}
