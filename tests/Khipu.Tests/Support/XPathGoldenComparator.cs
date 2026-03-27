namespace Khipu.Tests.Support;

using System.Text.Json;
using System.Xml;
using System.Xml.XPath;
using System.Globalization;

public static class XPathGoldenComparator
{
    public static IReadOnlyList<string> Compare(string xml, string goldenFixtureJson)
        => CompareDetailed(xml, goldenFixtureJson).Errors;

    public static XPathGoldenComparisonResult CompareDetailed(string xml, string goldenFixtureJson)
    {
        var fixture = JsonSerializer.Deserialize<XPathGoldenFixture>(goldenFixtureJson, JsonOptions)
                      ?? throw new InvalidOperationException("Invalid golden fixture JSON");

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        if (fixture.Namespaces is not null)
        {
            foreach (var kv in fixture.Namespaces)
            {
                nsmgr.AddNamespace(kv.Key, kv.Value);
            }
        }

        var errors = new List<string>();
        var unexpectedDeltas = new List<string>();

        if (fixture.Assertions.Count == 0)
        {
            errors.Add("Fixture has no assertions");
        }

        var assertionsByXPath = new Dictionary<string, XPathGoldenAssertion>(StringComparer.Ordinal);
        foreach (var assertion in fixture.Assertions)
        {
            if (string.IsNullOrWhiteSpace(assertion.XPath))
            {
                errors.Add("Assertion has empty XPath");
                continue;
            }

            if (!assertionsByXPath.TryAdd(assertion.XPath, assertion))
            {
                errors.Add($"Duplicate assertion XPath: {assertion.XPath}");
            }
        }

        var requiredXpaths = BuildRequiredXpaths(fixture);
        var requiredSet = requiredXpaths.ToHashSet(StringComparer.Ordinal);

        foreach (var assertionXPath in assertionsByXPath.Keys)
        {
            if (!requiredSet.Contains(assertionXPath))
            {
                errors.Add($"Assertion XPath not declared in requiredXpaths: {assertionXPath}");
            }
        }

        var valuesByXPath = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var requiredXPath in requiredXpaths)
        {
            if (!assertionsByXPath.ContainsKey(requiredXPath))
            {
                errors.Add($"Required XPath missing assertion: {requiredXPath}");
            }

            var nodeValue = doc.SelectSingleNode(requiredXPath, nsmgr)?.InnerText;
            valuesByXPath[requiredXPath] = nodeValue;
            if (nodeValue is null)
            {
                errors.Add($"Required XPath not found: {requiredXPath}");
            }
        }

        foreach (var assertion in assertionsByXPath.Values)
        {
            var value = valuesByXPath.TryGetValue(assertion.XPath, out var requiredValue)
                ? requiredValue
                : doc.SelectSingleNode(assertion.XPath, nsmgr)?.InnerText;

            if (value is null)
            {
                if (!requiredSet.Contains(assertion.XPath))
                {
                    errors.Add($"XPath not found: {assertion.XPath}");
                }

                continue;
            }

            if (!AreEquivalent(value, assertion.Expected))
            {
                var delta = $"Unexpected delta: {assertion.XPath} expected '{assertion.Expected}' actual '{value}'";
                unexpectedDeltas.Add(delta);
                errors.Add(delta);
            }
        }

        return new XPathGoldenComparisonResult(errors, unexpectedDeltas);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static bool AreEquivalent(string actual, string? expected)
    {
        var a = actual.Trim();
        var e = expected?.Trim() ?? string.Empty;

        if (string.Equals(a, e, StringComparison.Ordinal))
        {
            return true;
        }

        if (decimal.TryParse(a, NumberStyles.Any, CultureInfo.InvariantCulture, out var ad)
            && decimal.TryParse(e, NumberStyles.Any, CultureInfo.InvariantCulture, out var ed))
        {
            return ad == ed;
        }

        return false;
    }

    private static List<string> BuildRequiredXpaths(XPathGoldenFixture fixture)
    {
        var required = fixture.RequiredXpaths?.Where(static x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (required is { Count: > 0 })
        {
            return required;
        }

        return fixture.Assertions
            .Select(static x => x.XPath)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}

public sealed record XPathGoldenComparisonResult(
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> UnexpectedDeltas);

public sealed class XPathGoldenFixture
{
    public Dictionary<string, string>? Namespaces { get; init; }
    public List<string>? RequiredXpaths { get; init; }
    public List<XPathGoldenAssertion> Assertions { get; init; } = [];
}

public sealed class XPathGoldenAssertion
{
    public string XPath { get; init; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("equals")]
    public string? Expected { get; init; }
}
