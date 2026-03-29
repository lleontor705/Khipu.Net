# Validation

## Document validation engine

The validation engine checks documents against SUNAT business rules before sending.

```csharp
using Khipu.Validator.Engine;

var engine = new DocumentValidationEngine();

var result = engine.ValidateInvoice(invoice);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"[{error.Code}] {error.Path}: {error.Message}");
    }
}
```

### Supported document types

```csharp
var result = engine.ValidateInvoice(invoice);
var result = engine.ValidateSummary(summary);
var result = engine.ValidateVoided(voided);
```

### Error codes

| Code | Description |
|------|-------------|
| VAL-RUC-REQUIRED | RUC is required |
| VAL-RUC-INVALID | RUC fails checksum validation |
| VAL-SERIE-REQUIRED | Series is required |
| VAL-CORR-INVALID | Correlativo out of range (1-99999999) |
| VAL-DETAILS-EMPTY | No line items |
| VAL-TOTAL-NEGATIVE | Total amount is negative |
| VAL-DUPLICATE-REFERENCE | Duplicate document references |

## Individual field validation

Static methods for validating specific fields:

```csharp
using Khipu.Core.Validation;

// RUC validation (module 11 checksum)
bool valid = DocumentValidator.ValidateRuc("20123456789");

// DNI validation (8 digits)
bool valid = DocumentValidator.ValidateDni("12345678");

// Document validation by type
bool valid = DocumentValidator.ValidateDocument("6", "20123456789"); // RUC
bool valid = DocumentValidator.ValidateDocument("1", "12345678");    // DNI

// Series validation
bool valid = DocumentValidator.ValidateSerie("F001", "01"); // Factura: F prefix
bool valid = DocumentValidator.ValidateSerie("B001", "03"); // Boleta: B prefix

// Correlativo validation (1-99999999)
bool valid = DocumentValidator.ValidateCorrelativo(1);
```
