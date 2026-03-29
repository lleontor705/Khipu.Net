# Khipu.Net

.NET Core 10 library for SUNAT Peru Electronic Invoicing (Facturacion Electronica).
100% compatible with [Greenter PHP](https://github.com/thegreenter/greenter).

## Features

- Complete models for all SUNAT document types (Invoice, Receipt, CreditNote, DebitNote, Despatch, Summary, Voided, Retention, Perception)
- Factory and Builder patterns with fluent API
- Automatic tax calculation (IGV, ISC, IVAP, detractions, perceptions)
- XML UBL 2.1 generation with correct SUNAT namespaces
- XMLDSig digital signing with X.509 certificates
- SOAP client for SUNAT web services
- RUC/DNI validation

## Quick Start

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

## Packages

| Package | Description |
|---------|-------------|
| **Khipu.Net** | Meta-package - installs everything |
| **Khipu.Core** | Factory, Builder, Services, TaxCalculator, XmlSigner |
| **Khipu.Data** | Document models, entities, enums |
| **Khipu.Xml** | XML UBL 2.1 generation |
| **Khipu.Ws** | SUNAT SOAP client |
| **Khipu.Validator** | RUC/DNI/series validation |

## Documentation

Full documentation and examples: https://github.com/lleontor705/Khipu.Net

## License

MIT
