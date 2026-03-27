namespace Khipu.Tests;

using System.Text.Json;
using System.Text.RegularExpressions;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Validator.Engine;

public class ValidatorFixtureMatrixTests
{
    [Fact]
    public void ValidatorMatrix_FixturesMatchExpectedCodes()
    {
        var engine = new DocumentValidationEngine();
        var fixtures = JsonSerializer.Deserialize<List<ValidatorFixture>>(ReadFixture("validator-matrix.json"), JsonOptions) ?? [];

        foreach (var fixture in fixtures)
        {
            var result = fixture.Document switch
            {
                "invoice" => engine.ValidateInvoice(BuildInvoice(fixture.Mutations)),
                "summary" => engine.ValidateSummary(BuildSummary(fixture.Mutations)),
                "voided" => engine.ValidateVoided(BuildVoided(fixture.Mutations)),
                _ => throw new InvalidOperationException($"Unknown fixture document: {fixture.Document}")
            };

            Assert.Equal(fixture.ExpectValid, result.IsValid);

            var actualCodes = result.Errors.Select(e => e.Code).ToList();
            var expectedCodes = fixture.ExpectCodes ?? [];

            Assert.Equal(expectedCodes.Count, expectedCodes.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(actualCodes.Count, actualCodes.Distinct(StringComparer.Ordinal).Count());
            Assert.All(expectedCodes, code => Assert.Matches(CanonicalCodePattern, code));
            Assert.All(actualCodes, code => Assert.Matches(CanonicalCodePattern, code));
            Assert.Equal(expectedCodes, actualCodes);
        }
    }

    private static Invoice BuildInvoice(IReadOnlyList<string>? mutations)
    {
        var invoice = new Invoice
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "EMPRESA SAC" },
            Client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE SRL" },
            Serie = "F001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 3, 26),
            MtoImpVenta = 118,
            Details = [new SaleDetail { Codigo = "P001", Descripcion = "Producto", Cantidad = 1, MtoValorVenta = 100, PrecioVenta = 118 }]
        };

        ApplyCommonMutations(mutations, invoice);
        return invoice;
    }

    private static Summary BuildSummary(IReadOnlyList<string>? mutations)
    {
        var summary = new Summary
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "EMPRESA SAC" },
            Correlativo = "001",
            FechaGeneracion = new DateTime(2026, 3, 26),
            FechaEnvio = new DateTime(2026, 3, 27),
            Details = [new SummaryDetail { SerieNro = "B001-001", TipoDoc = VoucherType.Boleta, ClienteTipoDoc = "1", ClienteNroDoc = "12345678", MtoImpVenta = 118 }]
        };

        if (mutations?.Contains("duplicate-reference", StringComparer.OrdinalIgnoreCase) == true)
        {
            summary.Details.Add(new SummaryDetail { SerieNro = "B001-001", TipoDoc = VoucherType.Boleta, ClienteTipoDoc = "1", ClienteNroDoc = "87654321", MtoImpVenta = 50 });
        }

        if (mutations?.Contains("company-ruc-checksum-invalid", StringComparer.OrdinalIgnoreCase) == true)
        {
            summary.Company.Ruc = "20123456789";
        }

        return summary;
    }

    private static Voided BuildVoided(IReadOnlyList<string>? mutations)
    {
        var voided = new Voided
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "EMPRESA SAC" },
            Correlativo = "001",
            FechaGeneracion = new DateTime(2026, 3, 26),
            FechaEnvio = new DateTime(2026, 3, 27),
            Details = [new VoidedDetail { TipoDoc = "01", SerieNro = "F001-001", FechaDoc = new DateTime(2026, 3, 25), MotivoBaja = "Error" }]
        };

        if (mutations?.Contains("duplicate-reference", StringComparer.OrdinalIgnoreCase) == true)
        {
            voided.Details.Add(new VoidedDetail { TipoDoc = "01", SerieNro = "F001-001", FechaDoc = new DateTime(2026, 3, 24), MotivoBaja = "Duplicado" });
        }

        if (mutations?.Contains("company-ruc-checksum-invalid", StringComparer.OrdinalIgnoreCase) == true)
        {
            voided.Company.Ruc = "20123456789";
        }

        return voided;
    }

    private static void ApplyCommonMutations(IReadOnlyList<string>? mutations, Invoice invoice)
    {
        if (mutations is null) return;

        foreach (var mutation in mutations)
        {
            switch (mutation)
            {
                case "company-ruc-empty":
                    invoice.Company.Ruc = string.Empty;
                    break;
                case "details-empty":
                    invoice.Details.Clear();
                    break;
                case "company-ruc-checksum-invalid":
                    invoice.Company.Ruc = "20123456789";
                    break;
                case "serie-empty":
                    invoice.Serie = string.Empty;
                    break;
                case "correlativo-zero":
                    invoice.Correlativo = 0;
                    break;
                case "total-negative":
                    invoice.MtoImpVenta = -1;
                    break;
            }
        }
    }

    private static string ReadFixture(string relativePath)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly Regex CanonicalCodePattern = new("^VAL-[A-Z0-9]+(?:-[A-Z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
}

public sealed class ValidatorFixture
{
    public string Id { get; init; } = string.Empty;
    public string Document { get; init; } = string.Empty;
    public List<string>? Mutations { get; init; }
    public bool ExpectValid { get; init; }
    public List<string>? ExpectCodes { get; init; }
}
