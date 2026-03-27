namespace Khipu.Core.Builder;

using Khipu.Core.Algorithms;
using Khipu.Core.Interfaces;
using Khipu.Core.Validation;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Builder para facturas con cálculo completo de impuestos (paridad 100% Greenter)
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
    /// Cálculo completo de impuestos - Paridad 100% con Greenter
    /// Agrupa por tipo de afectación y suma impuestos por tipo (IGV, ISC, ICBPER, IVAP, OtrosTributos)
    /// </summary>
    private void CalculateTotals()
    {
        // Separar por tipo de afectación (Greenter: agrupación por tipo)
        var gravadas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Gravado).ToList();
        var exoneradas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Exonerado).ToList();
        var inafectas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Inafecto).ToList();
        var exportacion = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Exportacion).ToList();
        var gratuitas = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Gratuito).ToList();
        var ivap = _invoice.Details.Where(d => d.TipoAfectacionIgv == TaxType.Ivap).ToList();

        // Operaciones por tipo (sumas crudas, redondeo al final)
        _invoice.MtoOperGravadas = RoundingPolicy.RoundSunat(gravadas.Sum(d => d.MtoValorVenta));
        _invoice.MtoOperExoneradas = RoundingPolicy.RoundSunat(exoneradas.Sum(d => d.MtoValorVenta));
        _invoice.MtoOperInafectas = RoundingPolicy.RoundSunat(inafectas.Sum(d => d.MtoValorVenta));
        _invoice.MtoOperExportacion = RoundingPolicy.RoundSunat(exportacion.Sum(d => d.MtoValorVenta));
        _invoice.MtoOperGratuitas = RoundingPolicy.RoundSunat(gratuitas.Sum(d => d.MtoValorVenta));

        // IGV: de operaciones gravadas
        _invoice.MtoIGV = RoundingPolicy.RoundSunat(
            gravadas.Sum(d => d.Igv ?? TaxCalculator.CalculateIgv(d.MtoValorVenta)));

        // IGV Gratuitas
        _invoice.MtoIGVGratuitas = RoundingPolicy.RoundSunat(
            gratuitas.Sum(d => d.Igv ?? 0m));

        // IVAP
        _invoice.MtoBaseIvap = RoundingPolicy.RoundSunat(ivap.Sum(d => d.MtoValorVenta));
        _invoice.MtoIvap = RoundingPolicy.RoundSunat(
            ivap.Sum(d => d.Igv ?? (d.MtoValorVenta * (d.TasaIgv > 0 ? d.TasaIgv : 0.04m))));

        // ISC: de todos los detalles que tengan ISC
        _invoice.MtoBaseIsc = RoundingPolicy.RoundSunat(
            _invoice.Details.Where(d => d.MtoIsc.HasValue && d.MtoIsc > 0).Sum(d => d.MtoBaseIsc ?? d.MtoValorVenta));
        _invoice.MtoISC = RoundingPolicy.RoundSunat(
            _invoice.Details.Sum(d => d.MtoIsc ?? 0m));

        // Otros tributos
        _invoice.MtoBaseOth = RoundingPolicy.RoundSunat(
            _invoice.Details.Where(d => d.OtroTributo.HasValue && d.OtroTributo > 0).Sum(d => d.MtoBaseOth ?? d.MtoValorVenta));
        _invoice.MtoOtrosTributos = RoundingPolicy.RoundSunat(
            _invoice.Details.Sum(d => d.OtroTributo ?? 0m));

        // ICBPER
        _invoice.Icbper = RoundingPolicy.RoundSunat(
            _invoice.Details.Sum(d => d.Icbper ?? 0m));

        var descuentosRaw = _invoice.MtoDescuentos ?? 0m;

        // Invariantes Greenter:
        // 1) TotalImpuestos = IGV + ISC + IVAP + OtrosTributos + ICBPER
        // 2) MtoImpVenta = sum(operaciones) + TotalImpuestos - descuentos
        ApplyAggregateInvariants(RoundingPolicy.RoundSunat(descuentosRaw));
    }

    private void ApplyAggregateInvariants(decimal descuentos)
    {
        _invoice.TotalImpuestos = RoundingPolicy.RoundSunat(
            _invoice.MtoIGV + _invoice.MtoISC + _invoice.MtoIvap +
            _invoice.MtoOtrosTributos + _invoice.Icbper
        );

        var operaciones = _invoice.MtoOperGravadas +
                          _invoice.MtoOperExoneradas +
                          _invoice.MtoOperInafectas +
                          _invoice.MtoOperExportacion;

        _invoice.MtoImpVenta = RoundingPolicy.RoundSunat(
            operaciones + _invoice.TotalImpuestos - descuentos
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
            _invoice.Leyendas.Add(new Legend
            {
                Code = "1000",
                Value = AmountInWordsEsPe.Convert(_invoice.MtoImpVenta, _invoice.Moneda)
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

}
