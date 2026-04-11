namespace Khipu.Tests;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Generators;
using Khipu.Validator.Rules;
using Xunit;

public class ConstraintLoadersTests
{
    // ===== Company =====
    [Fact]
    public void LoadCompany_WithValidCompany_NoErrors()
    {
        var c = new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address { Ubigeo = "150101", CodigoLocal = "0000" } };
        Assert.Empty(ConstraintLoaders.LoadCompany(c));
    }

    [Fact]
    public void LoadCompany_WithInvalidRuc_ReturnsRegexError()
    {
        var c = new Company { Ruc = "ABC", RazonSocial = "TEST" };
        var errors = ConstraintLoaders.LoadCompany(c);
        Assert.Contains(errors, e => e.Code == "VAL-Regex");
    }

    [Fact]
    public void LoadCompany_WithNull_ReturnsNotNull()
    {
        var errors = ConstraintLoaders.LoadCompany(null);
        Assert.Contains(errors, e => e.Code == "VAL-NotNull");
    }

    // ===== Client =====
    [Fact]
    public void LoadClient_WithValidClient_NoErrors()
    {
        var c = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20100070970", RznSocial = "TEST" };
        Assert.Empty(ConstraintLoaders.LoadClient(c));
    }

    [Fact]
    public void LoadClient_WithLongName_ReturnsLengthError()
    {
        var c = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20100070970", RznSocial = new string('A', 101) };
        var errors = ConstraintLoaders.LoadClient(c);
        Assert.Contains(errors, e => e.Code == "VAL-Length");
    }

    // ===== Address =====
    [Fact]
    public void LoadAddress_WithValidAddress_NoErrors()
    {
        var a = new Address { Ubigeo = "150101", CodigoLocal = "0000", Direccion = "Test" };
        Assert.Empty(ConstraintLoaders.LoadAddress(a));
    }

    [Fact]
    public void LoadAddress_WithLongUbigeo_ReturnsError()
    {
        var a = new Address { Ubigeo = "1234567", CodigoLocal = "0000" };
        var errors = ConstraintLoaders.LoadAddress(a);
        Assert.Contains(errors, e => e.Code == "VAL-Length");
    }

    // ===== Invoice =====
    [Fact]
    public void LoadInvoice_WithValidInvoice_MinimalErrors()
    {
        var inv = DocumentGenerator.CreateInvoice();
        var errors = ConstraintLoaders.LoadInvoice(inv);
        // May have minor errors due to TipoOperacion being null
        Assert.DoesNotContain(errors, e => e.Path.Contains("company.ruc") && e.Code == "VAL-NotBlank");
    }

    [Fact]
    public void LoadInvoice_WithNull_ReturnsNotNull()
    {
        Assert.Contains(ConstraintLoaders.LoadInvoice(null), e => e.Code == "VAL-NotNull");
    }

    // ===== CreditNote/DebitNote =====
    [Fact]
    public void LoadCreditNote_WithValidNote_ChecksRequiredFields()
    {
        var n = DocumentGenerator.CreateCreditNote();
        var errors = ConstraintLoaders.LoadCreditNote(n);
        Assert.DoesNotContain(errors, e => e.Path == "creditNote.codMotivo" && e.Code == "VAL-NotBlank");
        Assert.DoesNotContain(errors, e => e.Path == "creditNote.numDocAfectado" && e.Code == "VAL-NotBlank");
    }

    [Fact]
    public void LoadDebitNote_WithEmptyMotivo_ReturnsError()
    {
        var n = DocumentGenerator.CreateDebitNote();
        n.CodMotivo = "";
        var errors = ConstraintLoaders.LoadDebitNote(n);
        Assert.Contains(errors, e => e.Path == "debitNote.codMotivo");
    }

    // ===== SaleDetail =====
    [Fact]
    public void LoadSaleDetail_WithLongDescription_ReturnsError()
    {
        var d = new SaleDetail { Descripcion = new string('X', 251), Unidad = "NIU", Cantidad = 1, MtoValorUnitario = 10, MtoValorVenta = 10 };
        var errors = ConstraintLoaders.LoadSaleDetail(d);
        Assert.Contains(errors, e => e.Code == "VAL-Length" && e.Path.Contains("descripcion"));
    }

    // ===== Despatch =====
    [Fact]
    public void LoadDespatch_WithValidDespatch_ChecksShipment()
    {
        var d = DocumentGenerator.CreateDespatch();
        var errors = ConstraintLoaders.LoadDespatch(d);
        Assert.DoesNotContain(errors, e => e.Path.Contains("envio.codTraslado") && e.Code == "VAL-NotBlank");
    }

    [Fact]
    public void LoadDespatch_WithMissingWeight_ReturnsError()
    {
        var d = DocumentGenerator.CreateDespatch();
        d.PesoTotal = null;
        var errors = ConstraintLoaders.LoadDespatch(d);
        Assert.Contains(errors, e => e.Path.Contains("pesoTotal"));
    }

    // ===== Direction =====
    [Fact]
    public void LoadDirection_WithValidDirection_NoErrors()
    {
        var a = new Address { Ubigeo = "150101", Direccion = "Av. Test 123" };
        Assert.Empty(ConstraintLoaders.LoadDirection(a));
    }

    // ===== Transportist =====
    [Fact]
    public void LoadTransportist_WithValidData_NoErrors()
    {
        var t = new Transportist { TipoDoc = "6", NumDoc = "20100070970", RznSocial = "TRANS SAC" };
        Assert.Empty(ConstraintLoaders.LoadTransportist(t));
    }

    [Fact]
    public void LoadTransportist_WithMissingDoc_ReturnsError()
    {
        var t = new Transportist { TipoDoc = "", NumDoc = "", RznSocial = "" };
        var errors = ConstraintLoaders.LoadTransportist(t);
        Assert.True(errors.Count >= 3);
    }

    // ===== Perception =====
    [Fact]
    public void LoadPerception_WithValidData_MinimalErrors()
    {
        var p = DocumentGenerator.CreatePerception();
        var errors = ConstraintLoaders.LoadPerception(p);
        Assert.DoesNotContain(errors, e => e.Path == "perception.serie" && e.Code == "VAL-NotBlank");
    }

    // ===== Retention =====
    [Fact]
    public void LoadRetention_WithValidData_MinimalErrors()
    {
        var r = DocumentGenerator.CreateRetention();
        var errors = ConstraintLoaders.LoadRetention(r);
        Assert.DoesNotContain(errors, e => e.Path == "retention.serie" && e.Code == "VAL-NotBlank");
    }

    // ===== Summary =====
    [Fact]
    public void LoadSummary_WithValidData_MinimalErrors()
    {
        var s = DocumentGenerator.CreateSummary();
        var errors = ConstraintLoaders.LoadSummary(s);
        Assert.DoesNotContain(errors, e => e.Path == "summary.correlativo" && e.Code == "VAL-NotBlank");
    }

    [Fact]
    public void LoadSummary_WithLongCorrelativo_ReturnsError()
    {
        var s = DocumentGenerator.CreateSummary();
        s.Correlativo = "123456";
        var errors = ConstraintLoaders.LoadSummary(s);
        Assert.Contains(errors, e => e.Code == "VAL-Length" && e.Path.Contains("correlativo"));
    }

    // ===== Voided =====
    [Fact]
    public void LoadVoided_WithValidData_MinimalErrors()
    {
        var v = DocumentGenerator.CreateVoided();
        var errors = ConstraintLoaders.LoadVoided(v);
        Assert.DoesNotContain(errors, e => e.Path == "voided.correlativo" && e.Code == "VAL-NotBlank");
    }

    [Fact]
    public void LoadVoidedDetail_WithMissingMotivo_ReturnsError()
    {
        var d = new VoidedDetail { TipoDoc = "01", SerieNro = "F001-1", MotivoBaja = "" };
        var errors = ConstraintLoaders.LoadVoidedDetail(d);
        Assert.Contains(errors, e => e.Path.Contains("desMotivoBaja"));
    }
}
