namespace Khipu.Validator.Contracts;

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationError> Errors);
