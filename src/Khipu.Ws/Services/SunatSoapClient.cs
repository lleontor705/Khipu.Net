namespace Khipu.Ws.Services;

using Khipu.Ws.Interfaces;
using Khipu.Ws.Models;

public class SunatSoapClient : ISunatClient
{
    public Task<SunatResponse> SendBillAsync(string xmlContent, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SunatResponse { Success = true });
    }

    public Task<SunatResponse> SendSummaryAsync(string xmlContent, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SunatResponse { Success = true, Ticket = "123" });
    }

    public Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TicketResponse { Success = true });
    }

    public Task<CdrResponse> GetCdrAsync(string ruc, string tipoComprobante, string serie, int correlativo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CdrResponse { Success = true });
    }
}
