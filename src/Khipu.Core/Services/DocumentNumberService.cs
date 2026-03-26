namespace Khipu.Core.Services;

using Khipu.Data.Documents;

/// <summary>
/// Servicio para generar números de documento
/// </summary>
public class DocumentNumberService
{
    private int _lastCorrelativo;

    public DocumentNumberService(int initialCorrelativo = 1)
    {
        _lastCorrelativo = initialCorrelativo;
    }

    /// <summary>
    /// Obtiene el siguiente correlativo
    /// </summary>
    public int GetNextCorrelativo()
    {
        return ++_lastCorrelativo;
    }

    /// <summary>
    /// Genera el número de documento (Serie-Correlativo)
    /// </summary>
    public string GenerateDocumentNumber(string serie, int correlativo)
    {
        return \$\"{serie}-{correlativo:D8}\";
    }

    /// <summary>
    /// Genera el nombre del archivo XML según SUNAT
    /// </summary>
    public string GenerateFileName(string ruc, string tipoDoc, string serie, int correlativo)
    {
        return \$\"{ruc}-{tipoDoc}-{serie}-{correlativo:D8}.xml\";
    }

    /// <summary>
    /// Genera el nombre del archivo ZIP según SUNAT
    /// </summary>
    public string GenerateZipName(string ruc, string tipoDoc, string serie, int correlativo)
    {
        return \$\"{ruc}-{tipoDoc}-{serie}-{correlativo:D8}.zip\";
    }
}
