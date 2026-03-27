namespace Khipu.Ws.Models;

public sealed record SunatSendRequest(
    byte[] ZipContent,
    string FileNameWithoutExtension,
    string? Ruc = null,
    string? TipoDocumento = null);

public sealed record CdrQuery(
    string Ruc,
    string TipoComprobante,
    string Serie,
    int Correlativo);
