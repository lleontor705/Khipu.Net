namespace Khipu.Validator.Rules;

using System.Text.RegularExpressions;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Validator.Contracts;

/// <summary>
/// Validaciones granulares por campo - Paridad Greenter validator loaders.
/// Cada método valida un tipo de entidad y retorna errores específicos por campo.
/// </summary>
public static partial class FieldValidators
{
    // ===== Client =====
    public static List<ValidationError> ValidateClient(Client? client, string prefix)
    {
        var errors = new List<ValidationError>();
        if (client == null)
        {
            errors.Add(new ValidationError("VAL-CLIENT-REQUIRED", prefix, "Datos del cliente son obligatorios"));
            return errors;
        }

        if (string.IsNullOrWhiteSpace(client.NumDoc))
            errors.Add(new ValidationError("VAL-CLIENT-NUMDOC-REQUIRED", $"{prefix}.numDoc", "Número de documento del cliente es obligatorio"));
        else
        {
            var tipoDoc = (int)client.TipoDoc;
            if (tipoDoc == 6 && !Regex.IsMatch(client.NumDoc, @"^\d{11}$")) // RUC
                errors.Add(new ValidationError("VAL-CLIENT-RUC-FORMAT", $"{prefix}.numDoc", "RUC del cliente debe tener 11 dígitos"));
            else if (tipoDoc == 1 && !Regex.IsMatch(client.NumDoc, @"^\d{8}$")) // DNI
                errors.Add(new ValidationError("VAL-CLIENT-DNI-FORMAT", $"{prefix}.numDoc", "DNI del cliente debe tener 8 dígitos"));
        }

        if (string.IsNullOrWhiteSpace(client.RznSocial))
            errors.Add(new ValidationError("VAL-CLIENT-NOMBRE-REQUIRED", $"{prefix}.rznSocial", "Nombre o razón social del cliente es obligatorio"));

        return errors;
    }

    // ===== Company =====
    public static List<ValidationError> ValidateCompany(Company? company, string prefix)
    {
        var errors = new List<ValidationError>();
        if (company == null)
        {
            errors.Add(new ValidationError("VAL-COMPANY-REQUIRED", prefix, "Datos de la empresa son obligatorios"));
            return errors;
        }

        if (string.IsNullOrWhiteSpace(company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-REQUIRED", $"{prefix}.ruc", "RUC es obligatorio"));
        else if (!Regex.IsMatch(company.Ruc, @"^(10|15|17|20)\d{9}$"))
            errors.Add(new ValidationError("VAL-RUC-PREFIX", $"{prefix}.ruc", "RUC debe iniciar con 10, 15, 17 o 20"));

        if (string.IsNullOrWhiteSpace(company.RazonSocial))
            errors.Add(new ValidationError("VAL-COMPANY-RAZON-REQUIRED", $"{prefix}.razonSocial", "Razón social es obligatoria"));

        if (company.Address != null)
            errors.AddRange(ValidateAddress(company.Address, $"{prefix}.address"));

        return errors;
    }

    // ===== Address =====
    public static List<ValidationError> ValidateAddress(Address? address, string prefix)
    {
        var errors = new List<ValidationError>();
        if (address == null) return errors;

        if (!string.IsNullOrEmpty(address.Ubigeo) && !Regex.IsMatch(address.Ubigeo, @"^\d{6}$"))
            errors.Add(new ValidationError("VAL-UBIGEO-FORMAT", $"{prefix}.ubigeo", "Ubigeo debe tener 6 dígitos"));

        return errors;
    }

    // ===== SaleDetail =====
    public static List<ValidationError> ValidateSaleDetail(SaleDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (detail.Cantidad <= 0)
            errors.Add(new ValidationError("VAL-DETAIL-CANTIDAD", $"{path}.cantidad", $"Línea {lineNumber}: cantidad debe ser mayor a cero"));

        if (string.IsNullOrWhiteSpace(detail.Descripcion))
            errors.Add(new ValidationError("VAL-DETAIL-DESCRIPCION", $"{path}.descripcion", $"Línea {lineNumber}: descripción es obligatoria"));

        if (string.IsNullOrWhiteSpace(detail.Unidad))
            errors.Add(new ValidationError("VAL-DETAIL-UNIDAD", $"{path}.unidad", $"Línea {lineNumber}: unidad de medida es obligatoria"));

        if (detail.MtoValorUnitario < 0)
            errors.Add(new ValidationError("VAL-DETAIL-VALORUNIT-NEGATIVE", $"{path}.mtoValorUnitario", $"Línea {lineNumber}: valor unitario no puede ser negativo"));

        if (detail.MtoValorVenta < 0)
            errors.Add(new ValidationError("VAL-DETAIL-VALORVENTA-NEGATIVE", $"{path}.mtoValorVenta", $"Línea {lineNumber}: valor venta no puede ser negativo"));

        return errors;
    }

    // ===== DespatchDetail =====
    public static List<ValidationError> ValidateDespatchDetail(DespatchDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (detail.Cantidad <= 0)
            errors.Add(new ValidationError("VAL-DETAIL-CANTIDAD", $"{path}.cantidad", $"Línea {lineNumber}: cantidad debe ser mayor a cero"));

        if (string.IsNullOrWhiteSpace(detail.Descripcion))
            errors.Add(new ValidationError("VAL-DETAIL-DESCRIPCION", $"{path}.descripcion", $"Línea {lineNumber}: descripción es obligatoria"));

        return errors;
    }

    // ===== Charge =====
    public static List<ValidationError> ValidateCharge(Charge? charge, int index, string prefix)
    {
        var errors = new List<ValidationError>();
        if (charge == null) return errors;

        if (charge.Monto.HasValue && charge.Monto < 0)
            errors.Add(new ValidationError("VAL-CHARGE-MONTO-NEGATIVE", $"{prefix}[{index}].monto", "Monto de cargo/descuento no puede ser negativo"));

        return errors;
    }

    // ===== PaymentTerms =====
    public static List<ValidationError> ValidatePaymentTerms(PaymentTerms? terms, string prefix)
    {
        var errors = new List<ValidationError>();
        if (terms == null) return errors;

        if (string.IsNullOrWhiteSpace(terms.Tipo))
            errors.Add(new ValidationError("VAL-PAYMENT-TIPO-REQUIRED", $"{prefix}.tipo", "Tipo de pago es obligatorio"));

        return errors;
    }

    // ===== Full Invoice Deep Validation =====
    public static List<ValidationError> ValidateInvoiceDeep(Invoice invoice)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateCompany(invoice.Company, "invoice.company"));
        errors.AddRange(ValidateClient(invoice.Client, "invoice.client"));

        if (invoice.FechaEmision == default)
            errors.Add(new ValidationError("VAL-FECHA-REQUIRED", "invoice.fechaEmision", "Fecha de emisión es obligatoria"));
        else if (invoice.FechaEmision > DateTime.Today.AddDays(1))
            errors.Add(new ValidationError("VAL-FECHA-FUTURE", "invoice.fechaEmision", "Fecha de emisión no puede ser futura"));

        if (string.IsNullOrWhiteSpace(invoice.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "invoice.serie", "Serie es obligatoria"));
        else if (!Regex.IsMatch(invoice.Serie, @"^[FB]\w{3}$"))
            errors.Add(new ValidationError("VAL-SERIE-FORMAT", "invoice.serie", "Serie debe empezar con F (factura) o B (boleta) seguido de 3 caracteres"));

        if (invoice.Correlativo <= 0 || invoice.Correlativo > 99999999)
            errors.Add(new ValidationError("VAL-CORR-RANGE", "invoice.correlativo", "Correlativo debe estar entre 1 y 99999999"));

        if (invoice.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "invoice.details", "Debe incluir al menos un detalle"));

        for (int i = 0; i < invoice.Details.Count; i++)
            errors.AddRange(ValidateSaleDetail(invoice.Details[i], i + 1, "invoice.details"));

        // Validar consistencia de totales
        if (invoice.MtoImpVenta < 0)
            errors.Add(new ValidationError("VAL-TOTAL-NEGATIVE", "invoice.mtoImpVenta", "Total no puede ser negativo"));

        if (invoice.MtoIGV < 0)
            errors.Add(new ValidationError("VAL-IGV-NEGATIVE", "invoice.mtoIGV", "IGV no puede ser negativo"));

        return errors;
    }

    // ===== SummaryDetail =====
    public static List<ValidationError> ValidateSummaryDetail(SummaryDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (string.IsNullOrWhiteSpace(detail.SerieNro))
            errors.Add(new ValidationError("VAL-SUMMARY-SERIENRO-REQUIRED", $"{path}.serieNro", $"Línea {lineNumber}: serie-número es obligatorio"));

        if (string.IsNullOrWhiteSpace(detail.ClienteNroDoc))
            errors.Add(new ValidationError("VAL-SUMMARY-CLIENTEDOC-REQUIRED", $"{path}.clienteNroDoc", $"Línea {lineNumber}: documento del cliente es obligatorio"));

        if (detail.Total < 0)
            errors.Add(new ValidationError("VAL-SUMMARY-TOTAL-NEGATIVE", $"{path}.total", $"Línea {lineNumber}: total no puede ser negativo"));

        var estado = detail.Estado;
        if (estado != "1" && estado != "2" && estado != "3")
            errors.Add(new ValidationError("VAL-SUMMARY-ESTADO-INVALID", $"{path}.estado", $"Línea {lineNumber}: estado debe ser 1 (Adicionar), 2 (Modificar) o 3 (Anular)"));

        return errors;
    }

    // ===== VoidedDetail =====
    public static List<ValidationError> ValidateVoidedDetail(VoidedDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (string.IsNullOrWhiteSpace(detail.SerieNro))
            errors.Add(new ValidationError("VAL-VOIDED-SERIENRO-REQUIRED", $"{path}.serieNro", $"Línea {lineNumber}: serie-número es obligatorio"));

        if (string.IsNullOrWhiteSpace(detail.TipoDoc))
            errors.Add(new ValidationError("VAL-VOIDED-TIPODOC-REQUIRED", $"{path}.tipoDoc", $"Línea {lineNumber}: tipo de documento es obligatorio"));

        if (string.IsNullOrWhiteSpace(detail.MotivoBaja))
            errors.Add(new ValidationError("VAL-VOIDED-MOTIVO-REQUIRED", $"{path}.motivoBaja", $"Línea {lineNumber}: motivo de baja es obligatorio"));

        return errors;
    }

    // ===== PerceptionDetail =====
    public static List<ValidationError> ValidatePerceptionDetail(PerceptionDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (string.IsNullOrWhiteSpace(detail.NumDoc))
            errors.Add(new ValidationError("VAL-PERCEP-NUMDOC-REQUIRED", $"{path}.numDoc", $"Línea {lineNumber}: número de documento es obligatorio"));

        if (detail.ImpTotal <= 0)
            errors.Add(new ValidationError("VAL-PERCEP-IMPTOTAL-INVALID", $"{path}.impTotal", $"Línea {lineNumber}: importe total debe ser mayor a cero"));

        if (detail.Porcentaje < 0 || detail.Porcentaje > 100)
            errors.Add(new ValidationError("VAL-PERCEP-PORCENTAJE-INVALID", $"{path}.porcentaje", $"Línea {lineNumber}: porcentaje debe estar entre 0 y 100"));

        return errors;
    }

    // ===== RetentionDetail =====
    public static List<ValidationError> ValidateRetentionDetail(RetentionDetail? detail, int lineNumber, string prefix)
    {
        var errors = new List<ValidationError>();
        if (detail == null) return errors;

        var path = $"{prefix}[{lineNumber}]";

        if (string.IsNullOrWhiteSpace(detail.NumDoc))
            errors.Add(new ValidationError("VAL-RETEN-NUMDOC-REQUIRED", $"{path}.numDoc", $"Línea {lineNumber}: número de documento es obligatorio"));

        if (detail.ImpTotal <= 0)
            errors.Add(new ValidationError("VAL-RETEN-IMPTOTAL-INVALID", $"{path}.impTotal", $"Línea {lineNumber}: importe total debe ser mayor a cero"));

        return errors;
    }

    // ===== Full Summary Deep Validation =====
    public static List<ValidationError> ValidateSummaryDeep(Summary summary)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateCompany(summary.Company, "summary.company"));

        if (summary.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "summary.details", "Debe incluir al menos un detalle"));

        for (int i = 0; i < summary.Details.Count; i++)
            errors.AddRange(ValidateSummaryDetail(summary.Details[i], i + 1, "summary.details"));

        // Check duplicates
        var dupes = summary.Details.GroupBy(d => d.SerieNro, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupes.Count > 0)
            errors.Add(new ValidationError("VAL-DUPLICATE-REFERENCE", "summary.details", $"Referencias duplicadas: {string.Join(", ", dupes)}"));

        return errors;
    }

    // ===== Full Perception Deep Validation =====
    public static List<ValidationError> ValidatePerceptionDeep(Perception perception)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateCompany(perception.Company, "perception.company"));
        errors.AddRange(ValidateClient(perception.Proveedor, "perception.proveedor"));

        if (string.IsNullOrWhiteSpace(perception.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "perception.serie", "Serie es obligatoria"));

        if (perception.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "perception.details", "Debe incluir al menos un detalle"));

        for (int i = 0; i < perception.Details.Count; i++)
            errors.AddRange(ValidatePerceptionDetail(perception.Details[i], i + 1, "perception.details"));

        return errors;
    }

    // ===== Full Retention Deep Validation =====
    public static List<ValidationError> ValidateRetentionDeep(Retention retention)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateCompany(retention.Company, "retention.company"));
        errors.AddRange(ValidateClient(retention.Proveedor, "retention.proveedor"));

        if (string.IsNullOrWhiteSpace(retention.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "retention.serie", "Serie es obligatoria"));

        if (retention.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "retention.details", "Debe incluir al menos un detalle"));

        for (int i = 0; i < retention.Details.Count; i++)
            errors.AddRange(ValidateRetentionDetail(retention.Details[i], i + 1, "retention.details"));

        return errors;
    }

    // ===== Full Despatch Deep Validation =====
    public static List<ValidationError> ValidateDespatchDeep(Despatch despatch)
    {
        var errors = new List<ValidationError>();

        errors.AddRange(ValidateCompany(despatch.Company, "despatch.company"));
        errors.AddRange(ValidateClient(despatch.Destinatario, "despatch.destinatario"));

        if (string.IsNullOrWhiteSpace(despatch.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "despatch.serie", "Serie es obligatoria"));
        else if (!Regex.IsMatch(despatch.Serie, @"^[TV]\w{3}$"))
            errors.Add(new ValidationError("VAL-SERIE-FORMAT", "despatch.serie", "Serie de guía debe empezar con T o V seguido de 3 caracteres"));

        if (string.IsNullOrWhiteSpace(despatch.CodMotivoTraslado))
            errors.Add(new ValidationError("VAL-MOTIVO-TRASLADO-REQUIRED", "despatch.codMotivoTraslado", "Motivo de traslado es obligatorio"));

        if (despatch.PesoTotal.HasValue && despatch.PesoTotal <= 0)
            errors.Add(new ValidationError("VAL-PESO-INVALID", "despatch.pesoTotal", "Peso total debe ser mayor a cero"));

        errors.AddRange(ValidateAddress(despatch.PuntoPartida, "despatch.puntoPartida"));
        errors.AddRange(ValidateAddress(despatch.PuntoLlegada, "despatch.puntoLlegada"));

        if (despatch.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "despatch.details", "Debe incluir al menos un detalle"));

        for (int i = 0; i < despatch.Details.Count; i++)
            errors.AddRange(ValidateDespatchDetail(despatch.Details[i], i + 1, "despatch.details"));

        return errors;
    }
}
