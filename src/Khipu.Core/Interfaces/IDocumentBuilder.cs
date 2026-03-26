namespace Khipu.Core.Interfaces;

/// <summary>
/// Interfaz para generación de documentos
/// </summary>
public interface IDocumentBuilder<T> where T : class
{
    T Build();
    bool Validate();
}
