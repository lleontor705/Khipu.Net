namespace Khipu.Validator.Engine;

using Khipu.Data.Documents;
using Khipu.Validator.Contracts;
using Khipu.Validator.Rules;

public class DocumentValidationEngine : IDocumentValidationEngine
{
    public ValidationResult ValidateInvoice(Invoice invoice)
    {
        return BuildResult(RuleCatalog.ValidateInvoice(invoice));
    }

    public ValidationResult ValidateSummary(Summary summary)
    {
        return BuildResult(RuleCatalog.ValidateSummary(summary));
    }

    public ValidationResult ValidateVoided(Voided voided)
    {
        return BuildResult(RuleCatalog.ValidateVoided(voided));
    }

    private static ValidationResult BuildResult(IReadOnlyList<ValidationError> errors)
    {
        var orderedErrors = RuleCatalog.ToCanonicalOrder(errors);
        return new ValidationResult(orderedErrors.Count == 0, orderedErrors);
    }
}
