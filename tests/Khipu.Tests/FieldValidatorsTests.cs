namespace Khipu.Tests;

using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Generators;
using Khipu.Validator.Rules;
using Xunit;

public class FieldValidatorsTests
{
    [Fact]
    public void ValidateClient_WithValidRuc_NoErrors()
    {
        var client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20100070970", RznSocial = "EMPRESA" };
        var errors = FieldValidators.ValidateClient(client, "test");
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateClient_WithInvalidRuc_ReturnsError()
    {
        var client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "123", RznSocial = "EMPRESA" };
        var errors = FieldValidators.ValidateClient(client, "test");
        Assert.Contains(errors, e => e.Code == "VAL-CLIENT-RUC-FORMAT");
    }

    [Fact]
    public void ValidateClient_WithInvalidDni_ReturnsError()
    {
        var client = new Client { TipoDoc = DocumentType.Dni, NumDoc = "123", RznSocial = "JUAN" };
        var errors = FieldValidators.ValidateClient(client, "test");
        Assert.Contains(errors, e => e.Code == "VAL-CLIENT-DNI-FORMAT");
    }

    [Fact]
    public void ValidateClient_WithNull_ReturnsRequired()
    {
        var errors = FieldValidators.ValidateClient(null, "test");
        Assert.Contains(errors, e => e.Code == "VAL-CLIENT-REQUIRED");
    }

    [Fact]
    public void ValidateClient_WithMissingName_ReturnsError()
    {
        var client = new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20100070970", RznSocial = "" };
        var errors = FieldValidators.ValidateClient(client, "test");
        Assert.Contains(errors, e => e.Code == "VAL-CLIENT-NOMBRE-REQUIRED");
    }

    [Fact]
    public void ValidateCompany_WithInvalidRucPrefix_ReturnsError()
    {
        var company = new Company { Ruc = "30100070970", RazonSocial = "TEST", Address = new Address() };
        var errors = FieldValidators.ValidateCompany(company, "test");
        Assert.Contains(errors, e => e.Code == "VAL-RUC-PREFIX");
    }

    [Fact]
    public void ValidateAddress_WithInvalidUbigeo_ReturnsError()
    {
        var address = new Address { Ubigeo = "12" };
        var errors = FieldValidators.ValidateAddress(address, "test");
        Assert.Contains(errors, e => e.Code == "VAL-UBIGEO-FORMAT");
    }

    [Fact]
    public void ValidateSaleDetail_WithZeroQuantity_ReturnsError()
    {
        var detail = new SaleDetail { Cantidad = 0, Descripcion = "Test", Unidad = "NIU" };
        var errors = FieldValidators.ValidateSaleDetail(detail, 1, "details");
        Assert.Contains(errors, e => e.Code == "VAL-DETAIL-CANTIDAD");
    }

    [Fact]
    public void ValidateSaleDetail_WithMissingDescription_ReturnsError()
    {
        var detail = new SaleDetail { Cantidad = 1, Descripcion = "", Unidad = "NIU" };
        var errors = FieldValidators.ValidateSaleDetail(detail, 1, "details");
        Assert.Contains(errors, e => e.Code == "VAL-DETAIL-DESCRIPCION");
    }

    [Fact]
    public void ValidateInvoiceDeep_WithValidInvoice_NoErrors()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        var errors = FieldValidators.ValidateInvoiceDeep(invoice);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateInvoiceDeep_WithMultipleErrors_ReturnsAll()
    {
        var invoice = new Invoice
        {
            Company = new Company { Ruc = "", RazonSocial = "" },
            Client = new Client { NumDoc = "", RznSocial = "" },
            Serie = "",
            Correlativo = 0,
            Details = new()
        };
        var errors = FieldValidators.ValidateInvoiceDeep(invoice);
        Assert.True(errors.Count >= 4);
    }

    [Fact]
    public void ValidateDespatchDeep_WithValidDespatch_NoErrors()
    {
        var despatch = DocumentGenerator.CreateDespatch();
        var errors = FieldValidators.ValidateDespatchDeep(despatch);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateInvoiceDeep_WithFutureDate_ReturnsError()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        invoice.FechaEmision = DateTime.Today.AddDays(30);
        var errors = FieldValidators.ValidateInvoiceDeep(invoice);
        Assert.Contains(errors, e => e.Code == "VAL-FECHA-FUTURE");
    }

    [Fact]
    public void ValidateInvoiceDeep_WithBadSerie_ReturnsError()
    {
        var invoice = DocumentGenerator.CreateInvoice();
        invoice.Serie = "X001";
        var errors = FieldValidators.ValidateInvoiceDeep(invoice);
        Assert.Contains(errors, e => e.Code == "VAL-SERIE-FORMAT");
    }
}
