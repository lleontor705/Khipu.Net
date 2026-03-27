using System.Text.Json;
using System.Text.RegularExpressions;

namespace Khipu.Tests;

public class ParityWorkflowBindingTests
{
    [Fact]
    public void BaselineGateFilters_StayBoundToExpectedSuiteSelectors()
    {
        var baselinePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "parity", "p6-pass-set.v1.json");
        using var baselineDoc = JsonDocument.Parse(File.ReadAllText(baselinePath));
        var gateFilter = baselineDoc.RootElement.GetProperty("gateFilter");

        Assert.Equal("TaxRoundingFixtureMatrixTests|TaxCalculatorTests|InvoiceBuilderTaxTests", gateFilter.GetProperty("P1").GetString());
        Assert.Equal("XmlGoldenFixtureTests", gateFilter.GetProperty("P2").GetString());
        Assert.Equal("ValidatorFixtureMatrixTests", gateFilter.GetProperty("P3").GetString());
        Assert.Equal("SunatSoapClientFixtureMatrixTests", gateFilter.GetProperty("P4").GetString());
        Assert.Equal("SunatServiceE2EParityTests", gateFilter.GetProperty("P5").GetString());
        Assert.Equal(
            "TaxRoundingFixtureMatrixTests|TaxCalculatorTests|InvoiceBuilderTaxTests|XmlGoldenFixtureTests|ValidatorFixtureMatrixTests|SunatSoapClientFixtureMatrixTests|SunatServiceE2EParityTests",
            gateFilter.GetProperty("combined").GetString());
    }

    [Fact]
    public void WorkflowUsesSharedFilteredGateRunner_ForP2ToP5()
    {
        var workflowPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "parity", "parity-phase-gates.yml");
        var yaml = File.ReadAllText(workflowPath);

        Assert.Contains("dotnet build Khipu.Net.slnx --no-restore", yaml, StringComparison.Ordinal);
        Assert.Contains("dotnet test Khipu.Net.slnx --no-restore --no-build", yaml, StringComparison.Ordinal);
        Assert.Contains("bash scripts/parity/run-filtered-gate.sh P1", yaml, StringComparison.Ordinal);
        Assert.Contains("bash scripts/parity/run-filtered-gate.sh P2", yaml, StringComparison.Ordinal);
        Assert.Contains("bash scripts/parity/run-filtered-gate.sh P3", yaml, StringComparison.Ordinal);
        Assert.Contains("bash scripts/parity/run-filtered-gate.sh P4", yaml, StringComparison.Ordinal);
        Assert.Contains("bash scripts/parity/run-filtered-gate.sh P5", yaml, StringComparison.Ordinal);
        Assert.Contains("python3 scripts/parity/compare-p6-pass-set.py", yaml, StringComparison.Ordinal);
        Assert.Contains("artifacts/parity/gate-summary.json", yaml, StringComparison.Ordinal);
        Assert.Contains("publish-gate-summary:", yaml, StringComparison.Ordinal);
    }

    [Fact]
    public void FilteredGateRunner_EnforcesNonEmptyExecutionGuard()
    {
        var scriptPath = ResolveFilteredGateRunnerPath();
        var script = File.ReadAllText(scriptPath);

        Assert.Matches(new Regex("\\.gateFilter\\[\\$phase\\]", RegexOptions.CultureInvariant), script);
        Assert.Contains("executed zero tests", script, StringComparison.Ordinal);
        Assert.Contains("dotnet test", script, StringComparison.Ordinal);
        Assert.Contains("--logger \"trx;LogFileName=", script, StringComparison.Ordinal);
    }

    private static string ResolveFilteredGateRunnerPath()
    {
        var directory = AppContext.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var pathParts = new List<string> { directory };
            pathParts.AddRange(Enumerable.Repeat("..", i + 1));
            pathParts.AddRange(["scripts", "parity", "run-filtered-gate.sh"]);
            var candidate = Path.GetFullPath(Path.Combine(pathParts.ToArray()));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Could not locate scripts/parity/run-filtered-gate.sh from test output directory.");
    }
}
