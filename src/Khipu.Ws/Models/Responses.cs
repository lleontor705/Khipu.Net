namespace Khipu.Ws.Models;

/// <summary>
/// Respuesta de envío de comprobante a SUNAT
/// </summary>
public class SunatResponse
{
    public bool Success { get; set; }
    public string? Ticket { get; set; }
    public byte[]? CdrZip { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? StatusCode { get; set; }
}

/// <summary>
/// Respuesta de consulta de CDR
/// </summary>
public class CdrResponse
{
    public bool Success { get; set; }
    public byte[]? CdrZip { get; set; }
    public string? CdrXml { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Detalle parseado del XML CDR de SUNAT - Paridad Greenter CdrResponse
/// </summary>
public class CdrDetail
{
    /// <summary>Identificador del documento (ReferenceID)</summary>
    public string? Id { get; set; }
    /// <summary>Código de respuesta SUNAT (0=aceptado, 2xxx=rechazado, 4xxx=observación)</summary>
    public string? Code { get; set; }
    /// <summary>Descripción de la respuesta</summary>
    public string? Description { get; set; }
    /// <summary>Notas adicionales de SUNAT</summary>
    public List<string>? Notes { get; set; }
    /// <summary>Referencia del documento (DocumentDescription)</summary>
    public string? Reference { get; set; }
    /// <summary>true si código es 0 o >= 4000 (aceptado/observado)</summary>
    public bool IsAccepted { get; set; }
}

/// <summary>
/// Respuesta de consulta de ticket
/// </summary>
public class TicketResponse
{
    public bool Success { get; set; }
    public string? Ticket { get; set; }
    public byte[]? CdrZip { get; set; }
    public string? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}
