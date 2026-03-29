# Khipu.Net

.NET 10 library for SUNAT Peru Electronic Invoicing. 100% compatible with [Greenter PHP](https://github.com/thegreenter/greenter).

## Install

```bash
dotnet add package Khipu.Net
```

## Features

- All SUNAT document types: Invoice, Receipt, CreditNote, DebitNote, Despatch, Summary, Voided, Retention, Perception
- Factory and Builder patterns with fluent API
- Automatic tax calculation: IGV (18%), ISC, IVAP, ICBPER, detractions, perceptions
- XML UBL 2.1 generation compliant with SUNAT
- XMLDSig digital signing with X.509 certificates
- SOAP client for SUNAT web services (Beta and Production)
- RUC/DNI validation with checksum verification
- SUNAT rounding rules (2 decimals, AwayFromZero)

## Quick start

```csharp
using Khipu.Core.Factory;
using Khipu.Core.Services;
using Khipu.Core.Security;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

// 1. Create factory
var factory = new InvoiceFactory(new Company
{
    Ruc = "20123456789",
    RazonSocial = "MI EMPRESA SAC"
});

// 2. Create invoice
var invoice = factory.CreateInvoice(
    client: new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SAC"
    },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

// 3. Add line items (taxes calculated automatically)
invoice.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Laptop HP ProBook",
    unidad: "NIU",
    cantidad: 2,
    valorUnitario: 1500.00m,
    tipoAfectacion: TaxType.Gravado
));

// 4. Sign and send to SUNAT
var signer = XmlSigner.FromPfx("certificate.pfx", "password");
var service = new SunatService(
    username: "20123456789MODDATOS",
    password: "moddatos",
    endpoint: SunatEndpoints.Beta,
    signer: signer
);

var response = await service.SendInvoiceAsync(invoice);
```

## Packages included

| Namespace | Description |
|-----------|-------------|
| `Khipu.Data` | Document models, entities, enums |
| `Khipu.Core` | Factory, Builder, TaxCalculator, XmlSigner, SunatService |
| `Khipu.Xml` | XML UBL 2.1 builders |
| `Khipu.Ws` | SUNAT SOAP client |
| `Khipu.Validator` | Document validation engine |

## Documentation

Full API reference, examples, and guides: https://github.com/lleontor705/Khipu.Net

## License

MIT
