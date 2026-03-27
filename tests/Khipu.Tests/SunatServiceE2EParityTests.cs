namespace Khipu.Tests;

using Khipu.Core.Services;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Ws.Helpers;
using Khipu.Ws.Interfaces;
using Khipu.Ws.Models;

public class SunatServiceE2EParityTests
{
    [Fact]
    public async Task SendInvoice_BuildSignZipDispatch_ProducesSunatContractPayload()
    {
        var fakeClient = new CapturingSunatClient();
        var service = new SunatService(fakeClient);
        var invoice = BuildInvoice();

        var response = await service.SendInvoiceAsync(invoice);

        Assert.True(response.Success);
        Assert.NotNull(fakeClient.LastSendBillRequest);
        Assert.Equal("20123456789-01-F001-00000123", fakeClient.LastSendBillRequest!.FileNameWithoutExtension);

        var xml = Khipu.Ws.Helpers.ZipHelper.ExtractXml(fakeClient.LastSendBillRequest.ZipContent);
        Assert.Contains("<cbc:ID>F001-123</cbc:ID>", xml);
        Assert.Contains("20123456789", xml);
        Assert.Contains("20987654321", xml);
    }

    [Fact]
    public async Task SendSummary_AndQueryCdr_FollowsExpectedParityFlow()
    {
        var fakeClient = new CapturingSunatClient();
        var service = new SunatService(fakeClient);

        var summary = new Summary
        {
            Company = new Company { Ruc = "20123456789", RazonSocial = "EMPRESA SAC" },
            Correlativo = "001",
            FechaGeneracion = new DateTime(2026, 3, 26),
            FechaEnvio = new DateTime(2026, 3, 27),
            Details = [new SummaryDetail { TipoDoc = VoucherType.Boleta, SerieNro = "B001-1", ClienteTipoDoc = "1", ClienteNroDoc = "12345678", MtoImpVenta = 50 }]
        };

        var send = await service.SendSummaryAsync(summary);
        var ticket = await service.GetStatusAsync(send.Ticket!);
        var cdr = await service.GetCdrAsync("20123456789", "01", "F001", 1);

        Assert.True(send.Success);
        Assert.NotNull(send.Ticket);
        Assert.True(ticket.Success);
        Assert.True(cdr.Success);
        Assert.True(cdr.IsAccepted);

        Assert.Equal(["SendSummary", "GetStatus", "GetCdr"], fakeClient.CallLog);
        Assert.Equal(3, fakeClient.CallLog.Count);

        Assert.NotNull(fakeClient.LastSendSummaryRequest);
        Assert.Equal("20123456789-RC-20260326-001", fakeClient.LastSendSummaryRequest!.FileNameWithoutExtension);
        Assert.NotEmpty(fakeClient.LastSendSummaryRequest.ZipContent);

        var summaryXml = ZipHelper.ExtractXml(fakeClient.LastSendSummaryRequest.ZipContent);
        Assert.Contains("<cbc:ID>001</cbc:ID>", summaryXml);
        Assert.Contains("<cbc:ReferenceDate>2026-03-26</cbc:ReferenceDate>", summaryXml);
        Assert.Contains("<cbc:IssueDate>2026-03-27</cbc:IssueDate>", summaryXml);
        Assert.Contains("<cbc:CustomerAssignedAccountID>20123456789</cbc:CustomerAssignedAccountID>", summaryXml);
        Assert.Contains("<cbc:DocumentTypeCode>03</cbc:DocumentTypeCode>", summaryXml);
        Assert.Contains("<cbc:ID>B001-1</cbc:ID>", summaryXml);

        Assert.Equal("TICKET-123", fakeClient.LastGetStatusTicket);

        Assert.NotNull(fakeClient.LastCdrQuery);
        Assert.Equal("20123456789", fakeClient.LastCdrQuery!.Ruc);
        Assert.Equal("01", fakeClient.LastCdrQuery.TipoComprobante);
        Assert.Equal("F001", fakeClient.LastCdrQuery.Serie);
        Assert.Equal(1, fakeClient.LastCdrQuery.Correlativo);
    }

    [Fact]
    public async Task SendSummaryAndQueryCdr_PollsUntilAccepted_ThenFetchesCdr()
    {
        var fakeClient = new CapturingSunatClient();
        fakeClient.EnqueueTicketStatus(new TicketResponse { Success = true, StatusCode = "98" });
        fakeClient.EnqueueTicketStatus(new TicketResponse { Success = true, StatusCode = "0" });

        var service = new SunatService(fakeClient);
        var summary = BuildSummary();

        var cdr = await service.SendSummaryAndQueryCdrAsync(summary, "01", "F001", 1, maxStatusChecks: 3, pollInterval: TimeSpan.Zero);

        Assert.True(cdr.Success);
        Assert.True(cdr.IsAccepted);
        Assert.Equal(["SendSummary", "GetStatus", "GetStatus", "GetCdr"], fakeClient.CallLog);
        Assert.Equal(2, fakeClient.GetStatusCalls);
    }

    [Fact]
    public async Task SendSummaryAndQueryCdr_RemainsPending_DoesNotFetchCdr()
    {
        var fakeClient = new CapturingSunatClient();
        fakeClient.EnqueueTicketStatus(new TicketResponse { Success = true, StatusCode = "98" });
        fakeClient.EnqueueTicketStatus(new TicketResponse { Success = true, StatusCode = "98" });

        var service = new SunatService(fakeClient);
        var summary = BuildSummary();

        var cdr = await service.SendSummaryAndQueryCdrAsync(summary, "01", "F001", 1, maxStatusChecks: 2, pollInterval: TimeSpan.Zero);

        Assert.False(cdr.Success);
        Assert.False(cdr.IsAccepted);
        Assert.Equal("98", cdr.ErrorCode);
        Assert.Contains("permanece pendiente", cdr.Notes);
        Assert.Equal(["SendSummary", "GetStatus", "GetStatus"], fakeClient.CallLog);
        Assert.Equal(0, fakeClient.GetCdrCalls);
    }

    [Fact]
    public async Task SendSummaryAndQueryCdr_WithoutCompanyRuc_FailsBeforeSending()
    {
        var fakeClient = new CapturingSunatClient();
        var service = new SunatService(fakeClient);
        var summary = BuildSummary();
        summary.Company = new Company { RazonSocial = "EMPRESA SAC" };

        var cdr = await service.SendSummaryAndQueryCdrAsync(summary, "01", "F001", 1, pollInterval: TimeSpan.Zero);

        Assert.False(cdr.Success);
        Assert.False(cdr.IsAccepted);
        Assert.Equal("INVALID_DOCUMENT", cdr.ErrorCode);
        Assert.Empty(fakeClient.CallLog);
    }

    private static Invoice BuildInvoice() => new()
    {
        Company = new Company
        {
            Ruc = "20123456789",
            RazonSocial = "EMPRESA SAC",
            Address = new Address
            {
                Ubigeo = "150101",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "LIMA",
                Direccion = "AV. PRINCIPAL 123",
                CodigoLocal = "0000"
            }
        },
        Client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE SRL", Address = new Address { Direccion = "JR. SECUNDARIO 456" } },
        Serie = "F001",
        Correlativo = 123,
        FechaEmision = new DateTime(2026, 3, 26, 12, 30, 0),
        Moneda = Currency.Pen,
        MtoOperGravadas = 200,
        MtoIGV = 36,
        MtoImpVenta = 236,
        Details = [new SaleDetail { Codigo = "PROD001", Descripcion = "Producto de prueba", Unidad = "NIU", Cantidad = 2, MtoValorUnitario = 100, MtoValorVenta = 200, PrecioVenta = 236 }]
    };

    private static Summary BuildSummary() => new()
    {
        Company = new Company { Ruc = "20123456789", RazonSocial = "EMPRESA SAC" },
        Correlativo = "001",
        FechaGeneracion = new DateTime(2026, 3, 26),
        FechaEnvio = new DateTime(2026, 3, 27),
        Details = [new SummaryDetail { TipoDoc = VoucherType.Boleta, SerieNro = "B001-1", ClienteTipoDoc = "1", ClienteNroDoc = "12345678", MtoImpVenta = 50 }]
    };

    private sealed class CapturingSunatClient : ISunatClient
    {
        private readonly Queue<TicketResponse> _ticketResponses = new();

        public List<string> CallLog { get; } = [];
        public SunatSendRequest? LastSendBillRequest { get; private set; }
        public SunatSendRequest? LastSendSummaryRequest { get; private set; }
        public string? LastGetStatusTicket { get; private set; }
        public CdrQuery? LastCdrQuery { get; private set; }
        public int GetStatusCalls { get; private set; }
        public int GetCdrCalls { get; private set; }

        public void EnqueueTicketStatus(TicketResponse response) => _ticketResponses.Enqueue(response);

        public Task<SunatResponse> SendBillAsync(SunatSendRequest request, CancellationToken cancellationToken = default)
        {
            CallLog.Add("SendBill");
            LastSendBillRequest = request;
            return Task.FromResult(new SunatResponse { Success = true, StatusCode = "0" });
        }

        public Task<SunatResponse> SendSummaryAsync(SunatSendRequest request, CancellationToken cancellationToken = default)
        {
            CallLog.Add("SendSummary");
            LastSendSummaryRequest = request;
            return Task.FromResult(new SunatResponse { Success = true, Ticket = "TICKET-123", StatusCode = "98" });
        }

        public Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken cancellationToken = default)
        {
            CallLog.Add("GetStatus");
            GetStatusCalls++;
            LastGetStatusTicket = ticket;

            if (_ticketResponses.TryDequeue(out var queued))
            {
                queued.Ticket ??= ticket;
                return Task.FromResult(queued);
            }

            return Task.FromResult(new TicketResponse { Success = true, Ticket = ticket, StatusCode = "0" });
        }

        public Task<CdrResponse> GetCdrAsync(CdrQuery query, CancellationToken cancellationToken = default)
        {
            CallLog.Add("GetCdr");
            GetCdrCalls++;
            LastCdrQuery = query;
            return Task.FromResult(new CdrResponse { Success = true, IsAccepted = true, ErrorCode = "0" });
        }
    }
}
