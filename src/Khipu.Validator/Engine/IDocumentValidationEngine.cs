namespace Khipu.Validator.Engine;

using Khipu.Data.Documents;
using Khipu.Validator.Contracts;

public interface IDocumentValidationEngine
{
    ValidationResult ValidateInvoice(Invoice invoice);
    ValidationResult ValidateSummary(Summary summary);
    ValidationResult ValidateVoided(Voided voided);
}
