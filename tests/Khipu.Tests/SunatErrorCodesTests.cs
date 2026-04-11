namespace Khipu.Tests;

using Khipu.Ws.Constants;
using Xunit;

public class SunatErrorCodesTests
{
    [Theory]
    [InlineData("0102", "Usuario o contraseña incorrectos")]
    [InlineData("0306", "No se puede leer (parsear) el archivo XML")]
    [InlineData("1001", "ID - El dato SERIE-CORRELATIVO no cumple con el formato de acuerdo al tipo de comprobante")]
    [InlineData("2017", "El numero de documento de identidad del receptor debe ser  RUC")]
    public void GetMessage_WithKnownCode_ReturnsMessage(string code, string expected)
    {
        var message = SunatErrorCodes.GetMessage(code);
        Assert.Equal(expected, message);
    }

    [Theory]
    [InlineData("9999")]
    [InlineData("0000")]
    [InlineData(null)]
    [InlineData("")]
    public void GetMessage_WithUnknownCode_ReturnsNull(string? code)
    {
        Assert.Null(SunatErrorCodes.GetMessage(code));
    }

    [Fact]
    public void GetMessageOrDefault_WithUnknownCode_ReturnsDefault()
    {
        var message = SunatErrorCodes.GetMessageOrDefault("9999", "Error desconocido");
        Assert.Equal("Error desconocido", message);
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("4000", true)]
    [InlineData("4001", true)]
    [InlineData("0102", false)]
    [InlineData("2017", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsAccepted_ReturnsCorrectResult(string? code, bool expected)
    {
        Assert.Equal(expected, SunatErrorCodes.IsAccepted(code));
    }

    [Theory]
    [InlineData("4000", true)]
    [InlineData("4014", true)]
    [InlineData("0", false)]
    [InlineData("2017", false)]
    public void IsObservation_ReturnsCorrectResult(string? code, bool expected)
    {
        Assert.Equal(expected, SunatErrorCodes.IsObservation(code));
    }

    [Theory]
    [InlineData("0102", true)]
    [InlineData("2017", true)]
    [InlineData("0", false)]
    [InlineData("4000", false)]
    public void IsRejection_ReturnsCorrectResult(string? code, bool expected)
    {
        Assert.Equal(expected, SunatErrorCodes.IsRejection(code));
    }

    [Theory]
    [InlineData("0", "Aceptado")]
    [InlineData("0102", "Autenticación/Proceso")]
    [InlineData("0306", "XML/Parsing")]
    [InlineData("1001", "Validación Factura")]
    [InlineData("2017", "Validación Impuestos/Firma")]
    [InlineData("2200", "Validación Percepción/Retención")]
    [InlineData("4000", "Observación (Aceptado)")]
    public void GetCategory_ReturnsCorrectCategory(string code, string expected)
    {
        Assert.Equal(expected, SunatErrorCodes.GetCategory(code));
    }

    [Fact]
    public void GetAll_ReturnsAll1710Codes()
    {
        var all = SunatErrorCodes.GetAll();
        Assert.NotEmpty(all);
        Assert.Equal(1710, all.Count); // 1710 códigos de greenter xcodes
    }

    [Fact]
    public void Count_Returns1710()
    {
        Assert.Equal(1710, SunatErrorCodes.Count);
    }

    [Theory]
    [InlineData("4000")] // Observación
    [InlineData("4339")] // Última observación
    [InlineData("3205")] // Validación campo
    [InlineData("1082")] // Guía remisión
    public void GetMessage_WithHighCodes_ReturnsMessage(string code)
    {
        var message = SunatErrorCodes.GetMessage(code);
        Assert.NotNull(message);
        Assert.NotEmpty(message);
    }
}
