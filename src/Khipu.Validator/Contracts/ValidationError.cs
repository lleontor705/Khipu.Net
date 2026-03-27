namespace Khipu.Validator.Contracts;

public sealed record ValidationError(
    string Code,
    string Path,
    string Message,
    string Severity = "Error");
