namespace Khipu.Tests;

using Khipu.Core.Validation;
using Xunit;

public class DocumentValidatorTests
{
    [Theory]
    [InlineData("20131312977", true)]  // RUC válido real (ejemplo conocido)
    [InlineData("12345678901", false)] // Prefijo inválido
    [InlineData("12345678", false)]    // Muy corto
    [InlineData("abcdefghijk", false)] // No numérico
    [InlineData("", false)]            // Vacío
    public void ValidateRuc_Tests(string ruc, bool expected)
    {
        var result = DocumentValidator.ValidateRuc(ruc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("12345678", true)]     // DNI válido
    [InlineData("87654321", true)]     // DNI válido
    [InlineData("1234567", false)]     // Muy corto
    [InlineData("123456789", false)]   // Muy largo
    [InlineData("abcdefgh", false)]    // No numérico
    [InlineData("", false)]            // Vacío
    public void ValidateDni_Tests(string dni, bool expected)
    {
        var result = DocumentValidator.ValidateDni(dni);
        Assert.Equal(expected, result);
    }
}
