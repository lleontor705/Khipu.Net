# Khipu.Net

.NET 10 library for SUNAT Peru Electronic Invoicing. Complete port of [Greenter PHP](https://github.com/thegreenter/greenter) with 100% parity.

## Install

```bash
dotnet add package Khipu.Net
```

## Features

- **9 document types**: Invoice, Receipt, CreditNote, DebitNote, Despatch, Summary, Voided, Retention, Perception
- **Dual transport**: SOAP client (legacy) + GRE REST API with OAuth2 (modern)
- **Tax calculation**: IGV 18%, ISC, IVAP, ICBPER, detractions, perceptions
- **XML UBL 2.1**: 9 builders + 8 parsers (XML to object)
- **Digital signature**: XMLDSig with X.509 certificates
- **CDR parsing**: Response reader with 1710 SUNAT error codes
- **HTML reports**: 9 document types + PDF export + QR codes
- **Validation**: 33 constraint loaders + field validators + engine
- **409 tests passing**

## Quick start

```csharp
using Khipu.Core.Builder;
using Khipu.Core.Services;
using Khipu.Core.Security;

var invoice = new InvoiceBuilder()
    .WithCompany(company)
    .WithClient(client)
    .WithSerie("F001")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Today)
    .AddDetail(new SaleDetail { ... })
    .Build();

var signer = XmlSigner.FromPfx("cert.pfx", "password");
var service = new SunatService("user", "pass", SunatEndpoints.Beta, signer);
var response = await service.SendInvoiceAsync(invoice);
```

## Modules

| Namespace | Description |
|-----------|-------------|
| `Khipu.Data` | Document models, entities, enums, generators |
| `Khipu.Core` | Builder, Factory, TaxCalculator, XmlSigner, HtmlReport, QR, PDF |
| `Khipu.Xml` | 9 XML builders UBL 2.1 + 8 parsers |
| `Khipu.Ws` | SOAP + GRE REST + CDR reader + 1710 error codes |
| `Khipu.Validator` | 33 constraint loaders + field validators |

## Documentation

https://github.com/lleontor705/Khipu.Net

## License

MIT
