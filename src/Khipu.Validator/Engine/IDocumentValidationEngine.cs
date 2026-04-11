namespace Khipu.Validator.Engine;

using Khipu.Data.Documents;
using Khipu.Validator.Contracts;

public interface IDocumentValidationEngine
{
    ValidationResult ValidateInvoice(Invoice invoice);
    ValidationResult ValidateCreditNote(CreditNote note);
    ValidationResult ValidateDebitNote(DebitNote note);
    ValidationResult ValidateDespatch(Despatch despatch);
    ValidationResult ValidatePerception(Perception perception);
    ValidationResult ValidateRetention(Retention retention);
    ValidationResult ValidateSummary(Summary summary);
    ValidationResult ValidateVoided(Voided voided);
}
