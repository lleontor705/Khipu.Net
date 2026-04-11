namespace Khipu.Data.Common;

/// <summary>
/// Documento adicional relacionado - Paridad Greenter AdditionalDoc
/// Usado en Despatch para documentos adicionales de referencia.
/// </summary>
public class AdditionalDoc
{
    /// <summary>Descripción del tipo de documento</summary>
    public string? TipoDesc { get; set; }
    /// <summary>Código de tipo de documento</summary>
    public string? Tipo { get; set; }
    /// <summary>Número de documento</summary>
    public string? Nro { get; set; }
    /// <summary>RUC del emisor</summary>
    public string? Emisor { get; set; }
}
