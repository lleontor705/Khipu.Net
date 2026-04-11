namespace Khipu.Tests;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Validator.Rules;
using Xunit;

public class ConstraintLoadersExtendedTests
{
    // ===== Cuota =====
    [Fact]
    public void LoadCuota_WithValid_NoErrors()
    {
        var c = new Cuota { Moneda = "PEN", Monto = 100, FechaPago = DateTime.Today };
        Assert.Empty(ConstraintLoaders.LoadCuota(c));
    }

    [Fact]
    public void LoadCuota_WithWrongMonedaLength_ReturnsError()
    {
        var c = new Cuota { Moneda = "PE", Monto = 100, FechaPago = DateTime.Today };
        Assert.Contains(ConstraintLoaders.LoadCuota(c), e => e.Code == "VAL-Length");
    }

    // ===== Detraction =====
    [Fact]
    public void LoadDetraction_WithValid_NoErrors()
    {
        var d = new Detraction { Porcentaje = 10, Monto = 100 };
        Assert.Empty(ConstraintLoaders.LoadDetraction(d));
    }

    [Fact]
    public void LoadDetraction_WithNull_Empty()
    {
        Assert.Empty(ConstraintLoaders.LoadDetraction(null));
    }

    // ===== Document =====
    [Fact]
    public void LoadDocument_WithValid_NoErrors()
    {
        var d = new Document { TipoDoc = "09", NroDoc = "T001-00000001" };
        Assert.Empty(ConstraintLoaders.LoadDocument(d));
    }

    [Fact]
    public void LoadDocument_WithLongNroDoc_ReturnsError()
    {
        var d = new Document { TipoDoc = "09", NroDoc = new string('A', 31) };
        Assert.Contains(ConstraintLoaders.LoadDocument(d), e => e.Code == "VAL-Length");
    }

    [Fact]
    public void LoadDocument_WithWrongTipoDocLen_ReturnsError()
    {
        var d = new Document { TipoDoc = "1" };
        Assert.Contains(ConstraintLoaders.LoadDocument(d), e => e.Code == "VAL-Length");
    }

    // ===== Legend =====
    [Fact]
    public void LoadLegend_WithValid_NoErrors()
    {
        var l = new Legend { Code = "1000", Value = "CIEN SOLES" };
        Assert.Empty(ConstraintLoaders.LoadLegend(l));
    }

    [Fact]
    public void LoadLegend_WithWrongCodeLength_ReturnsError()
    {
        var l = new Legend { Code = "10", Value = "TEST" };
        Assert.Contains(ConstraintLoaders.LoadLegend(l), e => e.Code == "VAL-Length");
    }

    [Fact]
    public void LoadLegend_WithLongValue_ReturnsError()
    {
        var l = new Legend { Code = "1000", Value = new string('X', 101) };
        Assert.Contains(ConstraintLoaders.LoadLegend(l), e => e.Code == "VAL-Length");
    }

    // ===== Prepayment =====
    [Fact]
    public void LoadPrepayment_WithValid_NoErrors()
    {
        var pp = new Prepayment { TipoDoc = "02", NroDoc = "F001-1", Total = 500 };
        Assert.Empty(ConstraintLoaders.LoadPrepayment(pp));
    }

    [Fact]
    public void LoadPrepayment_WithMissingTipoDoc_ReturnsError()
    {
        var pp = new Prepayment { TipoDoc = "", NroDoc = "F001-1", Total = 500 };
        Assert.Contains(ConstraintLoaders.LoadPrepayment(pp), e => e.Code == "VAL-NotBlank");
    }

    // ===== Payment =====
    [Fact]
    public void LoadPayment_WithValid_NoErrors()
    {
        var p = new Payment { FormaPago = "001", Monto = 1000 };
        Assert.Empty(ConstraintLoaders.LoadPayment(p));
    }

    [Fact]
    public void LoadPayment_WithMissingFormaPago_ReturnsError()
    {
        var p = new Payment { FormaPago = "", Monto = 1000 };
        Assert.Contains(ConstraintLoaders.LoadPayment(p), e => e.Code == "VAL-NotBlank");
    }

    // ===== SummaryPerception =====
    [Fact]
    public void LoadSummaryPerception_WithValid_NoErrors()
    {
        var sp = new SummaryPerception { CodReg = "01", Tasa = 2, MtoBase = 100, Mto = 2, MtoTotal = 102 };
        Assert.Empty(ConstraintLoaders.LoadSummaryPerception(sp));
    }

    [Fact]
    public void LoadSummaryPerception_WithMissingCodReg_ReturnsError()
    {
        var sp = new SummaryPerception { CodReg = "", Tasa = 2, MtoBase = 100, Mto = 2, MtoTotal = 102 };
        Assert.Contains(ConstraintLoaders.LoadSummaryPerception(sp), e => e.Code == "VAL-NotBlank");
    }

    // ===== FormaPago =====
    [Fact]
    public void LoadFormaPagoContado_WithValid_NoErrors()
    {
        var pt = new PaymentTerms { Tipo = "Contado" };
        Assert.Empty(ConstraintLoaders.LoadFormaPagoContado(pt));
    }

    [Fact]
    public void LoadFormaPagoCredito_WithValid_NoErrors()
    {
        var pt = new PaymentTerms { Tipo = "Credito", Moneda = "PEN", Monto = 500 };
        Assert.Empty(ConstraintLoaders.LoadFormaPagoCredito(pt));
    }

    [Fact]
    public void LoadFormaPagoCredito_WithBadMoneda_ReturnsError()
    {
        var pt = new PaymentTerms { Tipo = "Credito", Moneda = "PE", Monto = 500 };
        Assert.Contains(ConstraintLoaders.LoadFormaPagoCredito(pt), e => e.Code == "VAL-Length");
    }

    // ===== Charge =====
    [Fact]
    public void LoadCharge_WithValid_NoErrors()
    {
        var c = new Charge { Monto = 50, CodTipo = "00" };
        Assert.Empty(ConstraintLoaders.LoadCharge(c));
    }

    [Fact]
    public void LoadCharge_WithNegativeMonto_ReturnsError()
    {
        var c = new Charge { Monto = -10 };
        Assert.Contains(ConstraintLoaders.LoadCharge(c), e => e.Code == "VAL-Range");
    }
}
