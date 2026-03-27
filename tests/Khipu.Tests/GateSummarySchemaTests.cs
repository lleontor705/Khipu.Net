using System.Text.Json;
using System.Text.RegularExpressions;

namespace Khipu.Tests;

public class GateSummarySchemaTests
{
    [Fact]
    public void GateSummarySchema_DefinesDeterministicP0ToP6Phases()
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "parity", "gate-summary.schema.json");
        var json = File.ReadAllText(schemaPath);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var shaPattern = root.GetProperty("properties").GetProperty("sha").GetProperty("pattern").GetString();
        Assert.Equal("^[a-f0-9]{40}$", shaPattern);

        var prefixItems = root.GetProperty("properties").GetProperty("gates").GetProperty("prefixItems");
        Assert.Equal(7, prefixItems.GetArrayLength());

        for (var i = 0; i < prefixItems.GetArrayLength(); i++)
        {
            var expectedPhase = $"P{i}";
            var phaseConst = prefixItems[i]
                .GetProperty("allOf")[1]
                .GetProperty("properties")
                .GetProperty("phase")
                .GetProperty("const")
                .GetString();

            Assert.Equal(expectedPhase, phaseConst);
        }
    }

    [Fact]
    public void GateSummarySchema_RequiresAuditFieldsAndEvidencePattern()
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "parity", "gate-summary.schema.json");
        var json = File.ReadAllText(schemaPath);

        using var doc = JsonDocument.Parse(json);
        var gateDef = doc.RootElement.GetProperty("$defs").GetProperty("gate");

        var required = gateDef.GetProperty("required").EnumerateArray().Select(x => x.GetString()).ToHashSet();
        Assert.Contains("blocked_by", required);
        Assert.Contains("duration_ms", required);
        Assert.Contains("evidence_collected_at_utc", required);

        var evidencePattern = gateDef
            .GetProperty("properties")
            .GetProperty("evidence")
            .GetProperty("items")
            .GetProperty("pattern")
            .GetString();

        Assert.NotNull(evidencePattern);
        Assert.Matches(new Regex(evidencePattern!), "[PASS] ValidatorFixtureMatrixTests: all fixtures matched");
        Assert.DoesNotMatch(new Regex(evidencePattern!), "validator passed");
    }
}
