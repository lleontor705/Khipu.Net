namespace Khipu.Core.Builder;

using Khipu.Core.Constants;
using Khipu.Core.Interfaces;
using Khipu.Core.Validation;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Builder para facturas con cálculo completo de impuestos (basado en Greenter)
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
        AddDefaultLegends();
        return _invoice;
    }

    public bool Validate()
    {
        _errors.Clear();
        
        // Validar serie
        if (string.IsNullOrEmpty(_invoice.Serie))
            _errors.Add("Serie es requerida");
        else if (!DocumentValidator.ValidateSerie(_invoice.Serie, ((int)_invoice.TipoDoc).ToString().PadLeft(2, '0')))
            _errors.Add($"Serie {_invoice.Serie} no válida para factura");
        
        // Validar correlativo
        if (!DocumentValidator.ValidateCorrelativo(_invoice.Correlativo))
            _errors.Add("Correlativo debe estar entre 1 y 99999999");
        
        // Validar empresa
        if (!DocumentValidator.ValidateRuc(_invoice.Company.Ruc))
            _errors.Add("RUC de empresa inválido");
        
        // Validar cliente
        var clientTipoDoc = ((int)_invoice.Client.TipoDoc).ToString();
        if (!DocumentValidator.ValidateDocument(clientTipoDoc, _invoice.Client.NumDoc))
            _errors.Add("Documento de cliente inválido");
        
        // Validar detalles
        if (_invoice.Details.Count == 0)
            _errors.Add("Debe tener al menos un detalle");
        
        // Validar fecha
        if (_invoice.FechaEmision > DateTime.Now)
            _errors.Add("Fecha de emisión no puede ser futura");
        
        return _errors.Count == 0;
    }
    
    public List<string> GetErrors() => new(_errors);

    /// <summary>
    /// Cálculo completo de impuestos (basado en Greenter)
    /// </summary>
    private void CalculateTotals()
    {
        // Separar por tipo de afectación
        var gravadas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Gravado).ToList();
        var exoneradas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Exonerado).ToList();
        var inafectas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Inafecto).ToList();
        var exportacion = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Exportacion).ToList();
        
        // Operaciones gravadas
        _invoice.MtoOperGravadas = Math.Round(
            gravadas.Sum(d => d.MtoValorVenta),
            SunatConstants.DecimalesSunat
        );
        
        // Operaciones exoneradas
        _invoice.MtoOperExoneradas = Math.Round(
            exoneradas.Sum(d => d.MtoValorVenta),
            SunatConstants.DecimalesSunat
        );
        
        // Operaciones inafectas
        _invoice.MtoOperInafectas = Math.Round(
            inafectas.Sum(d => d.MtoValorVenta),
            SunatConstants.DecimalesSunat
        );
        
        // Exportación
        _invoice.MtoOperExportacion = Math.Round(
            exportacion.Sum(d => d.MtoValorVenta),
            SunatConstants.DecimalesSunat
        );
        
        // IGV (solo de operaciones gravadas)
        _invoice.MtoIGV = Math.Round(
            gravadas.Sum(d => d.MtoValorVenta * d.TasaIgv),
            SunatConstants.DecimalesSunat
        );
        
        // ISC
        _invoice.MtoISC = Math.Round(
            _invoice.Details.Sum(d => d.MtoIsc ?? 0),
            SunatConstants.DecimalesSunat
        );
        
        // Otros tributos
        _invoice.MtoOtrosTributos = Math.Round(
            _invoice.Details.Sum(d => d.Descuento ?? 0),
            SunatConstants.DecimalesSunat
        );
        
        // Total impuestos
        _invoice.TotalImpuestos = Math.Round(
            _invoice.MtoIGV + _invoice.MtoISC + _invoice.MtoOtrosTributos,
            SunatConstants.DecimalesSunat
        );
        
        // Descuentos globales
        var descuentos = _invoice.MtoDescuentos ?? 0;
        
        // Importe total de venta
        _invoice.MtoImpVenta = Math.Round(
            _invoice.MtoOperGravadas +
            _invoice.MtoOperExoneradas +
            _invoice.MtoOperInafectas +
            _invoice.MtoOperExportacion +
            _invoice.TotalImpuestos -
            descuentos,
            SunatConstants.DecimalesSunat
        );
    }
    
    /// <summary>
    /// Agregar leyendas por defecto (basado en Greenter)
    /// </summary>
    private void AddDefaultLegends()
    {
        if (_invoice.Leyendas == null)
            _invoice.Leyendas = new List<Legend>();
        
        // Leyenda de monto en letras
        if (_invoice.MtoImpVenta > 0)
        {
            var montoLetras = ConvertNumberToWords((decimal)_invoice.MtoImpVenta);
            _invoice.Leyendas.Add(new Legend
            {
                Code = "1000",
                Value = $"SON: {montoLetras} CON {(_invoice.MtoImpVenta % 1 * 100):00}/100 {_invoice.Moneda}"
            });
        }
        
        // Leyenda de detracción si aplica
        if (_invoice.Detraccion != null && _invoice.Detraccion.Mount > 0)
        {
            _invoice.Leyendas.Add(new Legend
            {
                Code = "2006",
                Value = "Operación sujeta a detracción"
            });
        }
    }
    
    /// <summary>
    /// Convierte número a letras (implementación básica)
    /// </summary>
    private static string ConvertNumberToWords(decimal number)
    {
        // TODO: Implementar conversión completa
        // Por ahora retornamos el número formateado
        return number.ToString("N2");
    }
}
