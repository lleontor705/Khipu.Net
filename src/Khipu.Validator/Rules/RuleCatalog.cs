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
