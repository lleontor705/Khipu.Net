namespace Khipu.Data.Interfaces;

using Khipu.Data.Entities;

/// <summary>
/// Interfaz base para todos los documentos electrónicos - Paridad Greenter DocumentInterface
/// </summary>
public interface IDocument
{
    Company Company { get; set; }
    string Serie { get; }
    DateTime FechaEmision { get; }

    /// <summary>
    /// Obtiene el nombre del archivo XML para el documento.
    /// Formato: RUC-TIPO-SERIE-CORRELATIVO
    /// </summary>
    string GetFileName();
}
