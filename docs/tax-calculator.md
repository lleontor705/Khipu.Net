# Tax Calculator

`TaxCalculator` provides static methods for SUNAT-compliant tax calculations. All results follow SUNAT rounding rules (2 decimal places, AwayFromZero).

## IGV calculation

```csharp
using Khipu.Core.Algorithms;

// Calculate IGV (18%)
var igv = TaxCalculator.CalculateIgv(1000);  // 180.00

// Calculate sale price from base amount
var price = TaxCalculator.CalculateSalePrice(1000, TaxType.Gravado);   // 1180.00
var price = TaxCalculator.CalculateSalePrice(1000, TaxType.Exonerado); // 1000.00
var price = TaxCalculator.CalculateSalePrice(1000, TaxType.Inafecto);  // 1000.00

// Reverse: calculate unit value from sale price
var value = TaxCalculator.CalculateUnitValue(1180, TaxType.Gravado);   // 1000.00
var value = TaxCalculator.CalculateUnitValue(1000, TaxType.Exonerado); // 1000.00
```

## Detractions

```csharp
// Default rate (10%)
var detraction = TaxCalculator.CalculateDetraction(5000);        // 500.00

// Custom rate
var detraction = TaxCalculator.CalculateDetraction(5000, 0.12m); // 600.00
```

## SUNAT rounding

```csharp
using Khipu.Core.Algorithms;

var rounded = RoundingPolicy.RoundSunat(100.125m);  // 100.13 (AwayFromZero)
var rounded = RoundingPolicy.RoundSunat(100.124m);  // 100.12
```

## Amount in words

```csharp
using Khipu.Core.Algorithms;

var words = AmountInWordsEsPe.Convert(1180.50m, Currency.Pen);
// "SON: MIL CIENTO OCHENTA CON 50/100 SOLES"

var words = AmountInWordsEsPe.Convert(500.00m, Currency.Usd);
// "SON: QUINIENTOS CON 00/100 DOLARES AMERICANOS"
```

## Tax rates (SunatConstants)

| Constant | Value | Description |
|----------|-------|-------------|
| `TasaIGV` | 0.18 | IGV rate (18%) |
| `TasaISC` | 0.10 | ISC rate (10%) |
| `TasaIVAP` | 0.04 | IVAP rate (4%) |
| `TasaPercepcion` | 0.02 | Perception rate (2%) |
| `TasaDetraccion` | 0.10 | Detraction rate (10%) |
| `DecimalesSunat` | 2 | Decimal places |
