namespace Khipu.Data.Enums;

/// <summary>
/// Tipos de documento de identidad (Catálogo 06)
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Registro Único de Contribuyentes
    /// </summary>
    Ruc = 6,
    
    /// <summary>
    /// Documento Nacional de Identidad
    /// </summary>
    Dni = 1,
    
    /// <summary>
    /// Carnet de Extranjería
    /// </summary>
    CarnetExtranjeria = 4,
    
    /// <summary>
    /// Pasaporte
    /// </summary>
    Pasaporte = 7,
    
    /// <summary>
    /// Sin RUC
    /// </summary>
    SinRuc = 0
}
