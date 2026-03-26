namespace Khipu.Core.Interfaces;

using Khipu.Data.Documents;

/// <summary>
/// Interfaz para builder de facturas
/// </summary>
public interface IInvoiceBuilder : IDocumentBuilder<Invoice>
{
    IInvoiceBuilder WithCompany(Data.Entities.Company company);
    IInvoiceBuilder WithClient(Data.Entities.Client client);
    IInvoiceBuilder WithSerie(string serie);
    IInvoiceBuilder WithCorrelativo(int correlativo);
    IInvoiceBuilder WithFechaEmision(DateTime fecha);
    IInvoiceBuilder AddDetail(SaleDetail detail);
}
