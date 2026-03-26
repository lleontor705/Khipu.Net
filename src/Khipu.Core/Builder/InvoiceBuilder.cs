namespace Khipu.Core.Builder;

using Khipu.Core.Constants;
using Khipu.Core.Interfaces;
using Khipu.Core.Validation;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Builder para facturas con validación completa
/// </summary>
public class InvoiceBuilder : IInvoiceBuilder
{
    private readonly Invoice _invoice = new();
    private readonly List<string> _errors = new();

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
        _errors.Clear();
        
        if (string.IsNullOrEmpty(_invoice.Serie))
            _errors.Add("Serie es requerida");
        
        if (_invoice.Correlativo <= 0)
            _errors.Add("Correlativo debe ser mayor a 0");
        
        if (!DocumentValidator.ValidateRuc(_invoice.Company.Ruc))
            _errors.Add("RUC de empresa inválido");
        
        if (!DocumentValidator.ValidateDocument(((int)_invoice.Client.TipoDoc).ToString(), _invoice.Client.NumDoc))
            _errors.Add("Documento de cliente inválido");
        
        if (_invoice.Details.Count == 0)
            _errors.Add("Debe tener al menos un detalle");
        
        return _errors.Count == 0;
    }
    
    public List<string> GetErrors() => new(_errors);

    private void CalculateTotals()
    {
        _invoice.MtoOperGravadas = Math.Round(
            _invoice.Details.Sum(d => d.MtoValorVenta), 
            SunatConstants.DecimalesSunat
        );
        
        _invoice.MtoIGV = Math.Round(
            _invoice.MtoOperGravadas * SunatConstants.TasaIGV, 
            SunatConstants.DecimalesSunat
        );
        
        _invoice.TotalImpuestos = _invoice.MtoIGV + _invoice.MtoISC + _invoice.MtoOtrosTributos;
        
        _invoice.MtoImpVenta = Math.Round(
            _invoice.MtoOperGravadas + 
            _invoice.MtoOperExoneradas + 
            _invoice.MtoOperInafectas + 
            _invoice.TotalImpuestos - 
            _invoice.Redondeo,
            SunatConstants.DecimalesSunat
        );
    }
}
