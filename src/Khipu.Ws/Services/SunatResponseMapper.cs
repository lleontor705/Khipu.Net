namespace Khipu.Ws.Services;

using Khipu.Ws.Helpers;
using Khipu.Ws.Models;

public sealed record SunatTransportPayload(
    string? Ticket = null,
    string? ApplicationResponse = null,
    string? StatusCode = null,
    string? StatusMessage = null,
    string? Content = null,
    string? Fault = null);

public static class SunatResponseMapper
{
    private const string PendingCode = "98";
    private const string SendAcceptedCode = "0";
    private const string GenericErrorStatusCode = "99";
    private const string SoapFaultCode = "SOAP_FAULT";
    private const string HttpErrorCode = "HTTP_ERROR";
    private const string TimeoutCode = "TIMEOUT";
    private const string UnknownCode = "UNKNOWN";
    private const string CdrParseErrorCode = "CDR_PARSE_ERROR";

    public static SunatResponse MapSend(SunatTransportPayload payload)
    {
        if (!string.IsNullOrWhiteSpace(payload.Fault))
        {
            return new SunatResponse
            {
                Success = false,
                ErrorCode = SoapFaultCode,
                ErrorMessage = payload.Fault,
                ResponseDate = DateTime.UtcNow,
            };
        }

        var ticket = Normalize(payload.Ticket);
        return new SunatResponse
        {
            Success = true,
            Ticket = ticket,
            CdrZip = DecodeBase64(payload.ApplicationResponse),
            ResponseDate = DateTime.UtcNow,
            StatusCode = ticket is null ? SendAcceptedCode : PendingCode,
        };
    }

    public static TicketResponse MapStatus(SunatTransportPayload payload, string ticket)
    {
        if (!string.IsNullOrWhiteSpace(payload.Fault))
        {
            return new TicketResponse
            {
                Success = false,
                ErrorMessage = payload.Fault,
                StatusCode = PendingCode,
            };
        }

        return new TicketResponse
        {
            Success = true,
            Ticket = ticket,
            StatusCode = string.IsNullOrWhiteSpace(payload.StatusCode) ? PendingCode : payload.StatusCode,
            CdrZip = DecodeBase64(payload.Content),
        };
    }

    public static CdrResponse MapCdr(SunatTransportPayload payload)
    {
        if (!string.IsNullOrWhiteSpace(payload.Fault))
        {
            return new CdrResponse
            {
                Success = false,
                ErrorCode = SoapFaultCode,
                ErrorMessage = payload.Fault,
                IsAccepted = false,
            };
        }

        try
        {
            var cdrZip = DecodeBase64(payload.Content);
            return new CdrResponse
            {
                Success = true,
                IsAccepted = string.Equals(payload.StatusCode, SendAcceptedCode, StringComparison.Ordinal),
                ErrorCode = payload.StatusCode,
                Notes = payload.StatusMessage,
                CdrZip = cdrZip,
                CdrXml = cdrZip is { Length: > 0 } ? ZipHelper.ExtractXml(cdrZip) : null,
            };
        }
        catch (InvalidDataException ex)
        {
            return CreateCdrTransportError(CdrParseErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            return CreateCdrTransportError(UnknownCode, ex.Message);
        }
    }

    public static SunatResponse MapSendTransportException(Exception exception)
    {
        Internal.Guard.NotNull(exception);

        return exception switch
        {
            HttpRequestException httpEx => CreateSendTransportError(HttpErrorCode, httpEx.Message),
            TaskCanceledException => CreateSendTransportError(TimeoutCode, "Tiempo de espera agotado"),
            _ => CreateSendTransportError(UnknownCode, exception.Message),
        };
    }

    public static TicketResponse MapStatusTransportException(Exception exception)
    {
        Internal.Guard.NotNull(exception);
        return CreateStatusTransportError(exception.Message);
    }

    public static CdrResponse MapCdrTransportException(Exception exception)
    {
        Internal.Guard.NotNull(exception);
        return CreateCdrTransportError(UnknownCode, exception.Message);
    }

    public static SunatResponse CreateSendTransportError(string errorCode, string errorMessage)
        => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };

    public static TicketResponse CreateStatusTransportError(string errorMessage)
        => new() { Success = false, StatusCode = GenericErrorStatusCode, ErrorMessage = errorMessage };

    public static CdrResponse CreateCdrTransportError(string errorCode, string errorMessage)
        => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage, IsAccepted = false };

    private static byte[]? DecodeBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
