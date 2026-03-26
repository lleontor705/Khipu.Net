namespace Khipu.Tests;

using Khipu.Core.Algorithms;
using Khipu.Core.Constants;
using Xunit;

public class TaxCalculatorTests
{
    [Fact]
    public void CalculateIgv_WithValidAmount_ReturnsCorrectIgv()
    {
        var baseAmount = 100m;
        var igv = TaxCalculator.CalculateIgv(baseAmount);
        Assert.Equal(18m, igv);
    }

    [Fact]
    public void CalculateSalePrice_Gravado_ReturnsWithIgv()
    {
        var baseAmount = 100m;
        var salePrice = TaxCalculator.CalculateSalePrice(baseAmount, TaxType.Gravado);
        Assert.Equal(118m, salePrice);
    }

    [Fact]
    public void CalculateSalePrice_Exonerado_ReturnsWithoutIgv()
    {
        var baseAmount = 100m;
        var salePrice = TaxCalculator.CalculateSalePrice(baseAmount, TaxType.Exonerado);
        Assert.Equal(100m, salePrice);
    }

    [Fact]
    public void CalculateUnitValue_Gravado_ReturnsWithoutIgv()
    {
        var salePrice = 118m;
        var unitValue = TaxCalculator.CalculateUnitValue(salePrice, TaxType.Gravado);
        Assert.Equal(100m, unitValue);
    }

    [Fact]
    public void CalculateDetraction_WithValidAmount_ReturnsCorrectDetraction()
    {
        var baseAmount = 1000m;
        var detraction = TaxCalculator.CalculateDetraction(baseAmount);
        Assert.Equal(100m, detraction); // 10%
    }

    [Theory]
    [InlineData(100, 18)]
    [InlineData(50, 9)]
    [InlineData(1000, 180)]
    public void CalculateIgv_MultipleAmounts_ReturnsCorrectValues(decimal baseAmount, decimal expectedIgv)
    {
        var igv = TaxCalculator.CalculateIgv(baseAmount);
        Assert.Equal(expectedIgv, igv);
    }
}
