namespace Khipu.Core.Builder;

using Khipu.Core.Interfaces;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Builder para facturas
/// </summary>
public class InvoiceBuilder : IInvoiceBuilder
{
    private readonly Invoice _invoice = new();
    private bool _isValid = true;

    public IInvoiceBuilder WithCompany(Company company)
    {
        _invoice.Company = company;
        return this;
    }

    public IInvoiceBuilder WithClient(Client client)
    {
        _invoice.Client = client;
        return this;
    }

    public IInvoiceBuilder WithSerie(string serie)
    {
        _invoice.Serie = serie;
        return this;
    }

    public IInvoiceBuilder WithCorrelativo(int correlativo)
    {
        _invoice.Correlativo = correlativo;
        return this;
    }

    public IInvoiceBuilder WithFechaEmision(DateTime fecha)
    {
        _invoice.FechaEmision = fecha;
        return this;
    }

    public IInvoiceBuilder AddDetail(SaleDetail detail)
    {
        _invoice.Details.Add(detail);
        return this;
    }

    public Invoice Build()
    {
        CalculateTotals();
        return _invoice;
    }

    public bool Validate()
    {
        _isValid = !string.IsNullOrEmpty(_invoice.Serie) &&
                   _invoice.Correlativo > 0 &&
                   !string.IsNullOrEmpty(_invoice.Company.Ruc) &&
                   !string.IsNullOrEmpty(_invoice.Client.NumDoc) &&
                   _invoice.Details.Count > 0;
        return _isValid;
    }

    private void CalculateTotals()
    {
        _invoice.MtoOperGravadas = _invoice.Details.Sum(d => d.MtoValorVenta);
        _invoice.MtoIGV = _invoice.MtoOperGravadas * 0.18m; // IGV 18%
        _invoice.MtoImpVenta = _invoice.MtoOperGravadas + _invoice.MtoIGV;
    }
}
