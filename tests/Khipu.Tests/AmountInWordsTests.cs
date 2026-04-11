namespace Khipu.Tests;

using Khipu.Core.Algorithms;
using Khipu.Data.Enums;
using Xunit;

public class AmountInWordsTests
{
    [Theory]
    [InlineData(0, "SON: CERO CON 00/100 SOLES")]
    [InlineData(1, "SON: UNO CON 00/100 SOLES")]
    [InlineData(10, "SON: DIEZ CON 00/100 SOLES")]
    [InlineData(15, "SON: QUINCE CON 00/100 SOLES")]
    [InlineData(16, "SON: DIECIseis CON 00/100 SOLES")]
    [InlineData(20, "SON: VEINTE CON 00/100 SOLES")]
    [InlineData(21, "SON: VEINTIuno CON 00/100 SOLES")]
    [InlineData(50, "SON: CINCUENTA CON 00/100 SOLES")]
    [InlineData(99, "SON: NOVENTA Y NUEVE CON 00/100 SOLES")]
    [InlineData(100, "SON: CIEN CON 00/100 SOLES")]
    [InlineData(118, "SON: CIENTO DIECIocho CON 00/100 SOLES")]
    [InlineData(500, "SON: QUINIENTOS CON 00/100 SOLES")]
    [InlineData(1000, "SON: MIL CON 00/100 SOLES")]
    [InlineData(1180, "SON: MIL CIENTO OCHENTA CON 00/100 SOLES")] // 1180 uses BuildHundreds, no lowercase
    [InlineData(5000, "SON: CINCO MIL CON 00/100 SOLES")]
    [InlineData(1000000, "SON: UN MILLÓN CON 00/100 SOLES")]
    public void Convert_WholeNumbers_ReturnsCorrectWords(decimal amount, string expected)
    {
        Assert.Equal(expected, AmountInWordsEsPe.Convert(amount, Currency.Pen));
    }

    [Fact]
    public void Convert_WithCents_IncludesCents()
    {
        var result = AmountInWordsEsPe.Convert(118.50m, Currency.Pen);
        Assert.Equal("SON: CIENTO DIECIocho CON 50/100 SOLES", result);
    }

    [Fact]
    public void Convert_WithUsd_ReturnsDolares()
    {
        var result = AmountInWordsEsPe.Convert(100, Currency.Usd);
        Assert.Contains("DÓLARES AMERICANOS", result);
    }

    [Fact]
    public void Convert_NegativeAmount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AmountInWordsEsPe.Convert(-1, Currency.Pen));
    }

    [Fact]
    public void Convert_WithRounding_RoundsCorrectly()
    {
        var result = AmountInWordsEsPe.Convert(99.999m, Currency.Pen);
        Assert.Contains("CIEN CON 00/100", result);
    }

    [Fact]
    public void Convert_LargeAmount_Works()
    {
        var result = AmountInWordsEsPe.Convert(2500000, Currency.Pen);
        Assert.Contains("DOS MILLONES QUINIENTOS MIL", result);
    }
}
