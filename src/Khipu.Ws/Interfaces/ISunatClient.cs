namespace Khipu.Ws.Interfaces;

using Khipu.Ws.Models;

/// <summary>
/// Interfaz para cliente de servicios SUNAT
/// </summary>
public interface ISunatClient
{
    Task<SunatResponse> SendBillAsync(string xmlContent, CancellationToken cancellationToken = default);
    Task<SunatResponse> SendSummaryAsync(string xmlContent, CancellationToken cancellationToken = default);
    Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken cancellationToken = default);
    Task<CdrResponse> GetCdrAsync(string ruc, string tipoComprobante, string serie, int correlativo, CancellationToken cancellationToken = default);
}
