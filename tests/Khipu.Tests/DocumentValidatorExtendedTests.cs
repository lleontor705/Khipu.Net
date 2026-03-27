namespace Khipu.Tests;

using Khipu.Core.Validation;

public class DocumentValidatorExtendedTests
{
    [Theory]
    [InlineData("20100070970", true)]
    [InlineData("20123456789", false)]
    [InlineData("20100070971", false)]
    [InlineData("12345678901", false)]
    [InlineData("2012345678", false)]
    [InlineData("201234567890", false)]
    [InlineData("20٠00000001", false)]
    [InlineData("abcdefghijk", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateRuc_Complete_Tests(string? ruc, bool expected)
    {
        var result = DocumentValidator.ValidateRuc(ruc ?? string.Empty);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1", "12345678", true)]
    [InlineData("6", "20100070970", true)]
    [InlineData("6", "20123456789", false)]
    [InlineData("6", "12345678", false)]
    [InlineData("1", "1234567", false)]
    [InlineData("4", "1234567890", true)]
    [InlineData("7", "AB123456", true)]
    [InlineData("0", "", true)]
    [InlineData("99", "123", false)]
    public void ValidateDocument_Tests(string tipoDoc, string numDoc, bool expected)
    {
        var result = DocumentValidator.ValidateDocument(tipoDoc, numDoc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("F001", "01", true)]
    [InlineData("B001", "03", true)]
    [InlineData("F002", "01", true)]
    [InlineData("X001", "01", false)]
    [InlineData("F01", "01", false)]
    [InlineData("", "01", false)]
    public void ValidateSerie_Tests(string serie, string tipoDoc, bool expected)
    {
        var result = DocumentValidator.ValidateSerie(serie, tipoDoc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(12345678, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(99999999, true)]
    [InlineData(100000000, false)]
    public void ValidateCorrelativo_Tests(int correlativo, bool expected)
    {
        var result = DocumentValidator.ValidateCorrelativo(correlativo);
        Assert.Equal(expected, result);
    }
}
