namespace Khipu.Tests;

using Khipu.Core.Validation;
using Xunit;

public class DocumentValidatorExtTests
{
    // ===== RUC edge cases =====
    [Theory]
    [InlineData("20100070970", true)]  // Valid RUC
    [InlineData("10467793549", true)]  // Valid persona natural
    [InlineData("30100070970", false)] // Invalid prefix
    [InlineData("00000000000", false)] // All zeros
    [InlineData("2010007097", false)]  // Too short
    [InlineData("201000709701", false)] // Too long
    [InlineData("ABCDEFGHIJK", false)] // Non-numeric
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateRuc_EdgeCases(string? ruc, bool expected)
    {
        Assert.Equal(expected, DocumentValidator.ValidateRuc(ruc!));
    }

    // ===== DNI edge cases =====
    [Theory]
    [InlineData("12345678", true)]
    [InlineData("00000001", true)]
    [InlineData("1234567", false)]   // Too short
    [InlineData("123456789", false)] // Too long
    [InlineData("ABCDEFGH", false)]  // Non-numeric
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateDni_EdgeCases(string? dni, bool expected)
    {
        Assert.Equal(expected, DocumentValidator.ValidateDni(dni!));
    }

    // ===== Serie validation =====
    [Theory]
    [InlineData("F001", "01", true)]  // Factura serie F
    [InlineData("B001", "03", true)]  // Boleta serie B
    [InlineData("FC01", "07", true)]  // Nota crédito F
    [InlineData("FD01", "08", true)]  // Nota débito F
    [InlineData("B001", "01", false)] // Factura con serie B = invalid
    [InlineData("F001", "03", false)] // Boleta con serie F = invalid
    [InlineData("F", "01", false)]    // Too short
    public void ValidateSerie_WithDifferentTipoDoc(string serie, string tipoDoc, bool expected)
    {
        Assert.Equal(expected, DocumentValidator.ValidateSerie(serie, tipoDoc));
    }

    // ===== Correlativo validation =====
    [Theory]
    [InlineData(1, true)]
    [InlineData(99999999, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(100000000, false)]
    public void ValidateCorrelativo_EdgeCases(int correlativo, bool expected)
    {
        Assert.Equal(expected, DocumentValidator.ValidateCorrelativo(correlativo));
    }
}
