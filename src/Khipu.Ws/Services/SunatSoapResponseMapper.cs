namespace Khipu.Ws.Services;

using Khipu.Ws.Models;

/// <summary>
/// Maps SOAP client response contracts to canonical classification outcomes.
/// </summary>
public static class SunatSoapResponseMapper
{
    public static SoapClassification Classify(SunatResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Success && response.CdrZip is { Length: > 0 } && string.IsNullOrWhiteSpace(response.Ticket))
        {
            return Success(SoapClassificationCode.SuccessCdr);
        }

        if (response.Success && !string.IsNullOrWhiteSpace(response.Ticket))
        {
            return Pending(SoapClassificationCode.PendingTicket);
        }

        if (!response.Success && string.Equals(response.ErrorCode, "SOAP_FAULT", StringComparison.Ordinal))
        {
            return Failure(SoapClassificationCode.Fault);
        }

        return Failure(SoapClassificationCode.Unknown);
    }

    public static SoapClassification Classify(TicketResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Success && string.Equals(response.StatusCode, "98", StringComparison.Ordinal))
        {
            return Pending(SoapClassificationCode.Pending);
        }

        if (!response.Success)
        {
            return Failure(SoapClassificationCode.Fault);
        }

        return Failure(SoapClassificationCode.Unknown);
    }

    public static SoapClassification Classify(CdrResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Success && string.Equals(response.ErrorCode, "CDR_PARSE_ERROR", StringComparison.Ordinal))
        {
            return Failure(SoapClassificationCode.CorruptCdr);
        }

        if (!response.Success && string.Equals(response.ErrorCode, "SOAP_FAULT", StringComparison.Ordinal))
        {
            return Failure(SoapClassificationCode.Fault);
        }

        if (!response.Success)
        {
            return Failure(SoapClassificationCode.Unknown);
        }

        if (string.Equals(response.ErrorCode, "0", StringComparison.Ordinal))
        {
            return Success(SoapClassificationCode.Accepted);
        }

        if (string.Equals(response.ErrorCode, "99", StringComparison.Ordinal))
        {
            return Success(SoapClassificationCode.Warning);
        }

        return Failure(SoapClassificationCode.Unknown);
    }

    private static SoapClassification Success(SoapClassificationCode code)
        => new(code, IsTerminal: true, IsSuccess: true);

    private static SoapClassification Pending(SoapClassificationCode code)
        => new(code, IsTerminal: false, IsSuccess: false);

    private static SoapClassification Failure(SoapClassificationCode code)
        => new(code, IsTerminal: true, IsSuccess: false);
}
