namespace Khipu.Ws.Models;

/// <summary>
/// Respuesta de envío de comprobante a SUNAT
/// </summary>
public class SunatResponse
{
    public bool Success { get; set; }
    public string? Ticket { get; set; }
    public string? CdrZip { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ResponseDate { get; set; }
}

/// <summary>
/// Respuesta de consulta de CDR
/// </summary>
public class CdrResponse
{
    public bool Success { get; set; }
    public string? CdrZip { get; set; }
    public string? CdrXml { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Respuesta de consulta de ticket
/// </summary>
public class TicketResponse
{
    public bool Success { get; set; }
    public string? Ticket { get; set; }
    public string? CdrZip { get; set; }
    public string? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}
