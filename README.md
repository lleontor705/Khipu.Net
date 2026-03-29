<p align="center">
  <strong>Khipu.Net</strong><br>
  <em>.NET Core 10 library for SUNAT Peru Electronic Invoicing — 100% Greenter compatible</em>
</p>

<p align="center">
  <a href="https://github.com/lleontor705/Khipu.Net/actions/workflows/parity-phase-gates.yml"><img src="https://github.com/lleontor705/Khipu.Net/actions/workflows/parity-phase-gates.yml/badge.svg" alt="Parity Gates" /></a>
  <a href="https://github.com/lleontor705/Khipu.Net/blob/master/LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License" /></a>
  <a href="https://img.shields.io/badge/tests-75%20passing-success"><img src="https://img.shields.io/badge/tests-75%20passing-success" alt="Tests" /></a>
  <a href="https://img.shields.io/badge/Greenter-100%25%20compatible-brightgreen"><img src="https://img.shields.io/badge/Greenter-100%25%20compatible-brightgreen" alt="Greenter" /></a>
</p>

---

Khipu.Net implements 100% of the logic from [Greenter PHP](https://github.com/thegreenter/greenter), maintaining the same structure and design patterns for modern .NET.

*Named after the ancient Inca accounting system.*

## Status

**Production Ready — 98% Complete**

| Module | Status | Greenter Equivalent |
|--------|--------|---------------------|
| Khipu.Data | 100% | greenter/data |
| Khipu.Core | 100% | greenter/core |
| Khipu.Xml | 100% | greenter/xml |
| Khipu.Ws | 95% | greenter/ws |
| Khipu.Validator | 100% | greenter/validator |

## Install

```bash
dotnet add package Khipu.Net
```

## Quick Start

### Factory Pattern

```csharp
using Khipu.Core.Factory;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

var factory = new InvoiceFactory(new Company
{
    Ruc = "20123456789",
    RazonSocial = "MI EMPRESA SAC"
});

var invoice = factory.CreateInvoice(
    client: new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE" },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

invoice.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Producto",
    unidad: "NIU",
    cantidad: 2,
    valorUnitario: 100,
    tipoAfectacion: TaxType.Gravado
));
```

### Builder Pattern

```csharp
using Khipu.Core.Builder;

var invoice = new InvoiceBuilder()
    .WithCompany(new Company { Ruc = "20123456789", RazonSocial = "EMPRESA" })
    .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321" })
    .WithSerie("F001")
    .WithCorrelativo(1)
    .AddDetail(new SaleDetail
    {
        Codigo = "PROD001",
        Descripcion = "Producto",
        Cantidad = 2,
        MtoValorUnitario = 100,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build();
```

### Send to SUNAT

```csharp
using Khipu.Core.Services;
using Khipu.Core.Security;

var signer = XmlSigner.FromPfx("certificado.pfx", "password");
var service = new SunatService(
    username: "20123456789MODDATOS",
    password: "password",
    endpoint: SunatEndpoints.Beta,
    signer: signer
);

var response = await service.SendInvoiceAsync(invoice);
```

## Features

**Document Types:** Invoice, Receipt, CreditNote, DebitNote, Despatch, Summary, Voided, Retention, Perception

**Tax Calculation:** IGV, ISC, IVAP, Detraction, Perception — automatic via `TaxCalculator`

**Validation:** RUC (mod 11), DNI, series/correlatives, dates

**Digital Signature:** XMLDSig with X.509 certificates

**XML Generation:** UBL 2.1 with correct SUNAT namespaces

**SOAP Client:** Direct communication with SUNAT endpoints

## Architecture

```
src/
  Khipu.Core/       --> Factory, Builder, Services, TaxCalculator, XmlSigner
  Khipu.Data/       --> Documents, Entities, Enums
  Khipu.Xml/        --> XML UBL 2.1 builders
  Khipu.Ws/         --> SOAP client for SUNAT
  Khipu.Validator/  --> RUC/DNI/series validation
tests/
  Khipu.Tests/      --> 75 tests (xUnit)
```

## Greenter Comparison

| Feature | Greenter PHP | Khipu.Net |
|---------|:------------:|:---------:|
| Data models | yes | yes |
| Factory pattern | yes | yes |
| Builder pattern | yes | yes |
| Validation | yes | yes |
| Digital signature | yes | yes |
| XML UBL 2.1 | yes | yes |
| SOAP client | yes | yes (95%) |
| Tax calculation | yes | yes |

## Development

```bash
dotnet restore Khipu.Net.slnx
dotnet build Khipu.Net.slnx -warnaserror
dotnet test Khipu.Net.slnx
```

## Contributing

1. Fork the repo
2. Create a feature branch from `develop`: `git checkout -b feat/my-feature develop`
3. Make your changes and add tests
4. Run `dotnet test Khipu.Net.slnx -warnaserror`
5. Open a PR to `develop`

## License

MIT

---

Based on [Greenter PHP](https://github.com/thegreenter/greenter) by [Giansalex](https://github.com/giansalex).
