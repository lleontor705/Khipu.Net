namespace Khipu.Tests;

using System.Globalization;
using System.Text.Json;
using Khipu.Core.Algorithms;
using Khipu.Data.Enums;

public class TaxRoundingFixtureMatrixTests
{
    [Fact]
    public void TaxRoundingMatrix_FixturesMatchExpectedValuesWithToleranceRules()
    {
        var fixtures = JsonSerializer.Deserialize<List<TaxRoundingFixture>>(ReadFixture("tax-rounding-matrix.json"), JsonOptions) ?? [];

        foreach (var fixture in fixtures)
        {
            var actual = Execute(fixture);
            AssertWithTolerance(fixture, actual);
        }
    }

    private static decimal Execute(TaxRoundingFixture fixture)
    {
        return fixture.Operation switch
        {
            "roundSunat" => RoundingPolicy.RoundSunat(fixture.Value ?? throw MissingField(fixture, nameof(fixture.Value))),
            "calculateIgv" => TaxCalculator.CalculateIgv(fixture.BaseAmount ?? throw MissingField(fixture, nameof(fixture.BaseAmount))),
            "calculateSalePrice" => TaxCalculator.CalculateSalePrice(
                fixture.BaseAmount ?? throw MissingField(fixture, nameof(fixture.BaseAmount)),
                ParseTaxType(fixture)),
            "calculateUnitValue" => TaxCalculator.CalculateUnitValue(
                fixture.SalePrice ?? throw MissingField(fixture, nameof(fixture.SalePrice)),
                ParseTaxType(fixture)),
            "calculateDetraction" => TaxCalculator.CalculateDetraction(
                fixture.BaseAmount ?? throw MissingField(fixture, nameof(fixture.BaseAmount)),
                fixture.Rate ?? 0.10m),
            _ => throw new InvalidOperationException($"Unknown tax operation in fixture '{fixture.Id}': {fixture.Operation}")
        };
    }

    private static void AssertWithTolerance(TaxRoundingFixture fixture, decimal actual)
    {
        var tolerance = ParseTolerance(fixture.Tolerance);
        var expected = fixture.Expected;
        var context = BuildContext(fixture, actual, expected);

        switch (tolerance.Kind)
        {
            case ToleranceKind.Exact:
                Assert.True(actual == expected, context + " (tolerance: exact)");
                break;

            case ToleranceKind.Absolute:
                var delta = Math.Abs(actual - expected);
                Assert.True(
                    delta <= tolerance.AbsoluteValue,
                    context + $" (tolerance: abs:{tolerance.AbsoluteValue.ToString(CultureInfo.InvariantCulture)}, delta:{delta.ToString(CultureInfo.InvariantCulture)})");
                break;

            default:
                throw new InvalidOperationException($"Unknown tolerance kind for fixture '{fixture.Id}'");
        }
    }

    private static TaxType ParseTaxType(TaxRoundingFixture fixture)
    {
        if (string.IsNullOrWhiteSpace(fixture.TaxType))
        {
            throw MissingField(fixture, nameof(fixture.TaxType));
        }

        if (Enum.TryParse<TaxType>(fixture.TaxType, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Invalid taxType '{fixture.TaxType}' in fixture '{fixture.Id}'");
    }

    private static ParsedTolerance ParseTolerance(string? tolerance)
    {
        if (string.IsNullOrWhiteSpace(tolerance) || tolerance.Equals("exact", StringComparison.OrdinalIgnoreCase))
        {
            return ParsedTolerance.Exact;
        }

        if (tolerance.StartsWith("abs:", StringComparison.OrdinalIgnoreCase))
        {
            var valueToken = tolerance[4..];
            if (!decimal.TryParse(valueToken, NumberStyles.Number, CultureInfo.InvariantCulture, out var absoluteValue) || absoluteValue < 0)
            {
                throw new FormatException($"Invalid absolute tolerance value: '{tolerance}'");
            }

            return ParsedTolerance.Absolute(absoluteValue);
        }

        throw new FormatException($"Unknown tolerance annotation: '{tolerance}'");
    }

    private static string BuildContext(TaxRoundingFixture fixture, decimal actual, decimal expected)
    {
        var note = string.IsNullOrWhiteSpace(fixture.Note) ? string.Empty : $" note='{fixture.Note}'";
        return $"[{fixture.Id}] op={fixture.Operation} expected={expected.ToString(CultureInfo.InvariantCulture)} actual={actual.ToString(CultureInfo.InvariantCulture)}{note}";
    }

    private static InvalidOperationException MissingField(TaxRoundingFixture fixture, string fieldName)
        => new($"Fixture '{fixture.Id}' requires field '{fieldName}' for operation '{fixture.Operation}'");

    private static string ReadFixture(string relativePath)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private enum ToleranceKind
    {
        Exact,
        Absolute
    }

    private readonly record struct ParsedTolerance(ToleranceKind Kind, decimal AbsoluteValue)
    {
        public static ParsedTolerance Exact => new(ToleranceKind.Exact, 0m);
        public static ParsedTolerance Absolute(decimal value) => new(ToleranceKind.Absolute, value);
    }
}

public sealed class TaxRoundingFixture
{
    public string Id { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public decimal? Value { get; init; }
    public decimal? BaseAmount { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? Rate { get; init; }
    public string? TaxType { get; init; }
    public decimal Expected { get; init; }
    public string? Tolerance { get; init; }
    public string? Note { get; init; }
}
