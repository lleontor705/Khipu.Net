namespace Khipu.Validator.Rules;

using Khipu.Core.Validation;
using Khipu.Data.Documents;
using Khipu.Validator.Contracts;

public static class RuleCatalog
{
    private static readonly IReadOnlyDictionary<string, int> CanonicalCodeOrder = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["VAL-RUC-REQUIRED"] = 0,
        ["VAL-RUC-INVALID"] = 0,
        ["VAL-SERIE-REQUIRED"] = 1,
        ["VAL-CORR-INVALID"] = 2,
        ["VAL-DETAILS-EMPTY"] = 3,
        ["VAL-TOTAL-NEGATIVE"] = 4,
        ["VAL-DUPLICATE-REFERENCE"] = 5
    };

    public static IReadOnlyList<ValidationError> ValidateInvoice(Invoice invoice)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(invoice.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-REQUIRED", "invoice.company.ruc", "RUC emisor es obligatorio"));
        else if (!DocumentValidator.ValidateRuc(invoice.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-INVALID", "invoice.company.ruc", "RUC emisor no cumple checksum SUNAT"));

        if (string.IsNullOrWhiteSpace(invoice.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "invoice.serie", "Serie es obligatoria"));

        if (invoice.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "invoice.correlativo", "Correlativo debe ser mayor a cero"));

        if (invoice.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "invoice.details", "Debe incluir al menos un detalle"));

        if (invoice.MtoImpVenta < 0)
            errors.Add(new ValidationError("VAL-TOTAL-NEGATIVE", "invoice.mtoImpVenta", "Total no puede ser negativo"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidateCreditNote(CreditNote note)
    {
        var errors = new List<ValidationError>();

        ValidateCompany(errors, note.Company, "creditNote");
        if (string.IsNullOrWhiteSpace(note.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "creditNote.serie", "Serie es obligatoria"));
        if (note.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "creditNote.correlativo", "Correlativo debe ser mayor a cero"));
        if (note.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "creditNote.details", "Debe incluir al menos un detalle"));
        if (string.IsNullOrWhiteSpace(note.NumDocAfectado))
            errors.Add(new ValidationError("VAL-DOC-AFECTADO-REQUIRED", "creditNote.numDocAfectado", "Documento afectado es obligatorio"));
        if (string.IsNullOrWhiteSpace(note.CodMotivo))
            errors.Add(new ValidationError("VAL-MOTIVO-REQUIRED", "creditNote.codMotivo", "Código de motivo es obligatorio"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidateDebitNote(DebitNote note)
    {
        var errors = new List<ValidationError>();

        ValidateCompany(errors, note.Company, "debitNote");
        if (string.IsNullOrWhiteSpace(note.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "debitNote.serie", "Serie es obligatoria"));
        if (note.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "debitNote.correlativo", "Correlativo debe ser mayor a cero"));
        if (note.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "debitNote.details", "Debe incluir al menos un detalle"));
        if (string.IsNullOrWhiteSpace(note.NumDocAfectado))
            errors.Add(new ValidationError("VAL-DOC-AFECTADO-REQUIRED", "debitNote.numDocAfectado", "Documento afectado es obligatorio"));
        if (string.IsNullOrWhiteSpace(note.CodMotivo))
            errors.Add(new ValidationError("VAL-MOTIVO-REQUIRED", "debitNote.codMotivo", "Código de motivo es obligatorio"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidateDespatch(Despatch despatch)
    {
        var errors = new List<ValidationError>();

        ValidateCompany(errors, despatch.Company, "despatch");
        if (string.IsNullOrWhiteSpace(despatch.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "despatch.serie", "Serie es obligatoria"));
        if (despatch.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "despatch.correlativo", "Correlativo debe ser mayor a cero"));
        if (despatch.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "despatch.details", "Debe incluir al menos un detalle"));
        if (string.IsNullOrWhiteSpace(despatch.CodMotivoTraslado))
            errors.Add(new ValidationError("VAL-MOTIVO-TRASLADO-REQUIRED", "despatch.codMotivoTraslado", "Motivo de traslado es obligatorio"));
        if (string.IsNullOrWhiteSpace(despatch.PuntoPartida?.Ubigeo) && string.IsNullOrWhiteSpace(despatch.PuntoPartida?.Direccion))
            errors.Add(new ValidationError("VAL-PARTIDA-REQUIRED", "despatch.puntoPartida", "Punto de partida es obligatorio"));
        if (string.IsNullOrWhiteSpace(despatch.PuntoLlegada?.Ubigeo) && string.IsNullOrWhiteSpace(despatch.PuntoLlegada?.Direccion))
            errors.Add(new ValidationError("VAL-LLEGADA-REQUIRED", "despatch.puntoLlegada", "Punto de llegada es obligatorio"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidatePerception(Perception perception)
    {
        var errors = new List<ValidationError>();

        ValidateCompany(errors, perception.Company, "perception");
        if (string.IsNullOrWhiteSpace(perception.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "perception.serie", "Serie es obligatoria"));
        if (perception.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "perception.correlativo", "Correlativo debe ser mayor a cero"));
        if (perception.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "perception.details", "Debe incluir al menos un detalle"));
        if (string.IsNullOrWhiteSpace(perception.Proveedor.NumDoc))
            errors.Add(new ValidationError("VAL-PROVEEDOR-REQUIRED", "perception.proveedor.numDoc", "Documento del proveedor es obligatorio"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidateRetention(Retention retention)
    {
        var errors = new List<ValidationError>();

        ValidateCompany(errors, retention.Company, "retention");
        if (string.IsNullOrWhiteSpace(retention.Serie))
            errors.Add(new ValidationError("VAL-SERIE-REQUIRED", "retention.serie", "Serie es obligatoria"));
        if (retention.Correlativo <= 0)
            errors.Add(new ValidationError("VAL-CORR-INVALID", "retention.correlativo", "Correlativo debe ser mayor a cero"));
        if (retention.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "retention.details", "Debe incluir al menos un detalle"));
        if (string.IsNullOrWhiteSpace(retention.Proveedor.NumDoc))
            errors.Add(new ValidationError("VAL-PROVEEDOR-REQUIRED", "retention.proveedor.numDoc", "Documento del proveedor es obligatorio"));

        return ToCanonicalOrder(errors);
    }

    private static void ValidateCompany(List<ValidationError> errors, Khipu.Data.Entities.Company company, string prefix)
    {
        if (string.IsNullOrWhiteSpace(company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-REQUIRED", $"{prefix}.company.ruc", "RUC emisor es obligatorio"));
        else if (!DocumentValidator.ValidateRuc(company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-INVALID", $"{prefix}.company.ruc", "RUC emisor no cumple checksum SUNAT"));
    }

    public static IReadOnlyList<ValidationError> ValidateSummary(Summary summary)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(summary.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-REQUIRED", "summary.company.ruc", "RUC emisor es obligatorio"));
        else if (!DocumentValidator.ValidateRuc(summary.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-INVALID", "summary.company.ruc", "RUC emisor no cumple checksum SUNAT"));

        if (summary.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "summary.details", "Debe incluir al menos un detalle"));

        if (summary.Details.GroupBy(d => d.SerieNro, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            errors.Add(new ValidationError("VAL-DUPLICATE-REFERENCE", "summary.details.serieNro", "Referencias duplicadas"));

        return ToCanonicalOrder(errors);
    }

    public static IReadOnlyList<ValidationError> ValidateVoided(Voided voided)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(voided.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-REQUIRED", "voided.company.ruc", "RUC emisor es obligatorio"));
        else if (!DocumentValidator.ValidateRuc(voided.Company.Ruc))
            errors.Add(new ValidationError("VAL-RUC-INVALID", "voided.company.ruc", "RUC emisor no cumple checksum SUNAT"));

        if (voided.Details.Count == 0)
            errors.Add(new ValidationError("VAL-DETAILS-EMPTY", "voided.details", "Debe incluir al menos un detalle"));

        if (voided.Details.GroupBy(d => d.SerieNro, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            errors.Add(new ValidationError("VAL-DUPLICATE-REFERENCE", "voided.details.serieNro", "Referencias duplicadas"));

        return ToCanonicalOrder(errors);
    }

    internal static IReadOnlyList<ValidationError> ToCanonicalOrder(IEnumerable<ValidationError> errors)
        => errors
            .OrderBy(error => CanonicalCodeOrder.TryGetValue(error.Code, out var ordinal) ? ordinal : int.MaxValue)
            .ThenBy(error => error.Path, StringComparer.Ordinal)
            .ToList();
}
