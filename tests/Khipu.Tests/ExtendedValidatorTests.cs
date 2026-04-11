namespace Khipu.Tests;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Validator.Engine;
using Xunit;

public class ExtendedValidatorTests
{
    private readonly DocumentValidationEngine _engine = new();

    [Fact]
    public void ValidateCreditNote_WithValidNote_Succeeds()
    {
        var note = TestDataFactory.CreateCreditNote();
        note.Company.Ruc = "20100070970"; // Valid RUC for checksum
        var result = _engine.ValidateCreditNote(note);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateCreditNote_WithMissingDocAfectado_Fails()
    {
        var note = TestDataFactory.CreateCreditNote();
        note.NumDocAfectado = "";
        var result = _engine.ValidateCreditNote(note);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-DOC-AFECTADO-REQUIRED");
    }

    [Fact]
    public void ValidateCreditNote_WithMissingMotivo_Fails()
    {
        var note = TestDataFactory.CreateCreditNote();
        note.CodMotivo = "";
        var result = _engine.ValidateCreditNote(note);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-MOTIVO-REQUIRED");
    }

    [Fact]
    public void ValidateDebitNote_WithValidNote_Succeeds()
    {
        var note = TestDataFactory.CreateDebitNote();
        note.Company.Ruc = "20100070970";
        var result = _engine.ValidateDebitNote(note);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDespatch_WithValidDespatch_Succeeds()
    {
        var despatch = TestDataFactory.CreateDespatch();
        despatch.Company.Ruc = "20100070970";
        var result = _engine.ValidateDespatch(despatch);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDespatch_WithMissingMotivoTraslado_Fails()
    {
        var despatch = TestDataFactory.CreateDespatch();
        despatch.CodMotivoTraslado = "";
        var result = _engine.ValidateDespatch(despatch);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-MOTIVO-TRASLADO-REQUIRED");
    }

    [Fact]
    public void ValidateDespatch_WithMissingPartida_Fails()
    {
        var despatch = TestDataFactory.CreateDespatch();
        despatch.PuntoPartida = new Address();
        var result = _engine.ValidateDespatch(despatch);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-PARTIDA-REQUIRED");
    }

    [Fact]
    public void ValidatePerception_WithValidPerception_Succeeds()
    {
        var perception = new Perception
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address() },
            Proveedor = new Client { NumDoc = "20987654321", RznSocial = "PROV" },
            Serie = "P001", Correlativo = 1,
            Details = new() { new PerceptionDetail { TipoDoc = "01", NumDoc = "F001-1" } }
        };
        var result = _engine.ValidatePerception(perception);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidatePerception_WithMissingProveedor_Fails()
    {
        var perception = new Perception
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address() },
            Proveedor = new Client { NumDoc = "", RznSocial = "" },
            Serie = "P001", Correlativo = 1,
            Details = new() { new PerceptionDetail { TipoDoc = "01", NumDoc = "F001-1" } }
        };
        var result = _engine.ValidatePerception(perception);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-PROVEEDOR-REQUIRED");
    }

    [Fact]
    public void ValidateRetention_WithValidRetention_Succeeds()
    {
        var retention = new Retention
        {
            Company = new Company { Ruc = "20100070970", RazonSocial = "TEST", Address = new Address() },
            Proveedor = new Client { NumDoc = "20987654321", RznSocial = "PROV" },
            Serie = "R001", Correlativo = 1,
            Details = new() { new RetentionDetail { TipoDoc = "01", NumDoc = "F001-1" } }
        };
        var result = _engine.ValidateRetention(retention);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateRetention_WithInvalidRuc_Fails()
    {
        var retention = new Retention
        {
            Company = new Company { Ruc = "12345678901", RazonSocial = "TEST", Address = new Address() },
            Proveedor = new Client { NumDoc = "20987654321", RznSocial = "PROV" },
            Serie = "R001", Correlativo = 1,
            Details = new() { new RetentionDetail { TipoDoc = "01", NumDoc = "F001-1" } }
        };
        var result = _engine.ValidateRetention(retention);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "VAL-RUC-INVALID");
    }
}
