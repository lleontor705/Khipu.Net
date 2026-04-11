namespace Khipu.Tests;

using Khipu.Core.Algorithms;
using Xunit;

public class RoundingPolicyTests
{
    [Theory]
    [InlineData(10.005, 10.01)] // Midpoint rounds away from zero
    [InlineData(10.004, 10.00)]
    [InlineData(10.015, 10.02)] // Banker's rounding NOT used (AwayFromZero)
    [InlineData(0.125, 0.13)]
    [InlineData(99.999, 100.00)]
    [InlineData(0, 0)]
    [InlineData(1.1, 1.10)]
    [InlineData(-10.005, -10.01)] // Negative: rounds away from zero
    public void RoundSunat_RoundsTo2Decimals_AwayFromZero(decimal input, decimal expected)
    {
        Assert.Equal(expected, RoundingPolicy.RoundSunat(input));
    }

    [Fact]
    public void RoundSunat_PreservesExactValues()
    {
        Assert.Equal(100.00m, RoundingPolicy.RoundSunat(100.00m));
        Assert.Equal(18.00m, RoundingPolicy.RoundSunat(18.00m));
    }

    [Fact]
    public void RoundSunat_HandlesVerySmallValues()
    {
        Assert.Equal(0.01m, RoundingPolicy.RoundSunat(0.009m));
        Assert.Equal(0.00m, RoundingPolicy.RoundSunat(0.004m));
    }
}
