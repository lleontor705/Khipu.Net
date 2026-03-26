namespace Khipu.Tests;

using Khipu.Core.Validation;
using Xunit;

public class DocumentValidatorExtendedTests
{
    [Theory]
    [InlineData(\"20131312977\", true)]  // RUC válido real
    [InlineData(\"20123456789\", true)]  // RUC válido ficticio
    [InlineData(\"12345678901\", false)] // Prefijo inválido
    [InlineData(\"2012345678\", false)]  // Muy corto
    [InlineData(\"201234567890\", false)] // Muy largo
    [InlineData(\"abcdefghijk\", false)] // No numérico
    [InlineData(\"\", false)]            // Vacío
    [InlineData(null, false)]           // Null
    public void ValidateRuc_Complete_Tests(string ruc, bool expected)
    {
        var result = DocumentValidator.ValidateRuc(ruc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(\"1\", \"12345678\", true)]     // DNI válido
    [InlineData(\"6\", \"20123456789\", true)]  // RUC válido
    [InlineData(\"6\", \"12345678\", false)]    // RUC inválido
    [InlineData(\"1\", \"1234567\", false)]     // DNI muy corto
    [InlineData(\"4\", \"1234567890\", true)]   // Carnet extranjería
    [InlineData(\"7\", \"AB123456\", true)]     // Pasaporte
    [InlineData(\"0\", \"\", true)]             // Sin RUC
    [InlineData(\"99\", \"123\", false)]        // Tipo inválido
    public void ValidateDocument_Tests(string tipoDoc, string numDoc, bool expected)
    {
        var result = DocumentValidator.ValidateDocument(tipoDoc, numDoc);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(\"F001\", \"01\", true)]   // Factura
    [InlineData(\"B001\", \"03\", true)]   // Boleta
    [InlineData(\"F002\", \"01\", true)]   // Factura serie 2
    [InlineData(\"X001\", \"01\", false)]  // Prefijo inválido
    [InlineData(\"F01\", \"01\", false)]   // Muy corto
    [InlineData(\"\", \"01\", false)]      // Vacío
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
