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

    public ValidationResult ValidateCreditNote(CreditNote note)
    {
        return BuildResult(RuleCatalog.ValidateCreditNote(note));
    }

    public ValidationResult ValidateDebitNote(DebitNote note)
    {
        return BuildResult(RuleCatalog.ValidateDebitNote(note));
    }

    public ValidationResult ValidateDespatch(Despatch despatch)
    {
        return BuildResult(RuleCatalog.ValidateDespatch(despatch));
    }

    public ValidationResult ValidatePerception(Perception perception)
    {
        return BuildResult(RuleCatalog.ValidatePerception(perception));
    }

    public ValidationResult ValidateRetention(Retention retention)
    {
        return BuildResult(RuleCatalog.ValidateRetention(retention));
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
