namespace Khipu.Xml.Interfaces;

/// <summary>
/// Interfaz para generadores de XML
/// </summary>
public interface IXmlBuilder<T> where T : class
{
    string Build(T document);
    string GetFileName(T document);
}
