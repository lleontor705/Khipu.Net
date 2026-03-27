namespace Khipu.Core.Algorithms;

using Khipu.Core.Constants;

/// <summary>
/// Shared SUNAT rounding policy for monetary values.
/// </summary>
public static class RoundingPolicy
{
    public static decimal RoundSunat(decimal value)
    {
        return Math.Round(value, SunatConstants.DecimalesSunat, MidpointRounding.AwayFromZero);
    }
}
