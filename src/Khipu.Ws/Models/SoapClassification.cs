namespace Khipu.Ws.Models;

/// <summary>
/// Canonical SOAP/CDR outcome used for parity assertions.
/// </summary>
public enum SoapClassificationCode
{
    SuccessCdr,
    PendingTicket,
    Pending,
    Accepted,
    Warning,
    Fault,
    CorruptCdr,
    Unknown,
}

/// <summary>
/// Canonical classification contract for SOAP/CDR outcomes.
/// </summary>
public sealed record SoapClassification(
    SoapClassificationCode Code,
    bool IsTerminal,
    bool IsSuccess);
