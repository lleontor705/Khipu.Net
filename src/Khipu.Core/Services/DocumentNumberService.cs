namespace Khipu.Core.Services;

public class DocumentNumberService
{
    private int _lastCorrelativo;

    public DocumentNumberService(int initialCorrelativo = 1)
    {
        _lastCorrelativo = initialCorrelativo;
    }

    public int GetNextCorrelativo() => ++_lastCorrelativo;

    public string GenerateDocumentNumber(string serie, int correlativo)
        => $"{serie}-{correlativo:D8}";

    public string GenerateFileName(string ruc, string tipoDoc, string serie, int correlativo)
        => $"{ruc}-{tipoDoc}-{serie}-{correlativo:D8}.xml";

    public string GenerateZipName(string ruc, string tipoDoc, string serie, int correlativo)
        => $"{ruc}-{tipoDoc}-{serie}-{correlativo:D8}.zip";
}
