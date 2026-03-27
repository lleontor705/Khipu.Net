namespace Khipu.Ws.Interfaces;

using Khipu.Ws.Models;

/// <summary>
/// Interfaz para cliente de servicios SUNAT
/// </summary>
public interface ISunatClient
{
    Task<SunatResponse> SendBillAsync(SunatSendRequest request, CancellationToken cancellationToken = default);
    Task<SunatResponse> SendSummaryAsync(SunatSendRequest request, CancellationToken cancellationToken = default);
    Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken cancellationToken = default);
    Task<CdrResponse> GetCdrAsync(CdrQuery query, CancellationToken cancellationToken = default);
}
