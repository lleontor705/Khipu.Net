<p align="center">
  <img src="docs/assets/logo.svg" alt="Khipu.Net" width="420"/>
</p>

<p align="center">
  <img src="docs/assets/badge-dotnet.svg" alt=".NET 10"/>
  <img src="docs/assets/badge-tests.svg" alt="409 tests passing"/>
  <img src="docs/assets/badge-greenter.svg" alt="Greenter 100% parity"/>
  <img src="docs/assets/badge-license.svg" alt="MIT License"/>
</p>

<p align="center">
  <strong>Facturación Electrónica SUNAT Perú para .NET</strong><br/>
  Port completo de <a href="https://github.com/thegreenter/greenter">Greenter PHP</a> con paridad 100%.
</p>

---

## Arquitectura

<p align="center">
  <img src="docs/assets/architecture.svg" alt="Architecture" width="680"/>
</p>

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Package** | `Khipu.Net` | Metapaquete NuGet que agrupa todo |
| **Validation** | `Khipu.Validator` | 33 constraint loaders, field validators, engine |
| **Business** | `Khipu.Core` | Builder, Factory, TaxCalculator, XmlSigner, HtmlReport, QR, PDF |
| **XML** | `Khipu.Xml` | 9 builders UBL 2.1 + 8 parsers XML-to-object |
| **Services** | `Khipu.Ws` | SOAP client, GRE REST client, CDR reader, 1710 error codes |
| **Models** | `Khipu.Data` | 9 document types, entities, enums, generators (zero dependencies) |

---

## Inicio rapido

### Instalacion

```bash
dotnet add package Khipu.Net
```

### Crear y enviar una factura

```csharp
using Khipu.Core.Builder;
using Khipu.Core.Services;
using Khipu.Core.Security;
using Khipu.Data.Entities;
using Khipu.Data.Documents;
using Khipu.Data.Enums;
using Khipu.Ws.Constants;

// Construir factura con calculo automatico de impuestos
var invoice = new InvoiceBuilder()
    .WithCompany(new Company
    {
        Ruc = "20123456789",
        RazonSocial = "MI EMPRESA SAC",
        Address = new Address
        {
            Ubigeo = "150101", Direccion = "AV. PRINCIPAL 123",
            Departamento = "LIMA", Provincia = "LIMA",
            Distrito = "LIMA", CodigoLocal = "0000"
        }
    })
    .WithClient(new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SRL"
    })
    .WithSerie("F001")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Today)
    .AddDetail(new SaleDetail
    {
        Codigo = "PROD001",
        Descripcion = "Laptop HP ProBook",
        Unidad = "NIU",
        Cantidad = 1,
        MtoValorUnitario = 2500,
        MtoValorVenta = 2500,
        PrecioVenta = 2950,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build();

// Firmar y enviar a SUNAT
var signer = XmlSigner.FromPfx("certificado.pfx", "password");
var service = new SunatService("20123456789MODDATOS", "clavesol", SunatEndpoints.Beta, signer);

var response = await service.SendInvoiceAsync(invoice);
```

---

## Tipos de documento

Los 9 tipos de comprobante electronico de SUNAT:

```csharp
await service.SendInvoiceAsync(invoice);        // Factura (01)
await service.SendReceiptAsync(receipt);         // Boleta (03)
await service.SendCreditNoteAsync(creditNote);   // Nota de Credito (07)
await service.SendDebitNoteAsync(debitNote);      // Nota de Debito (08)
await service.SendDespatchAsync(despatch);        // Guia de Remision (09)
await service.SendSummaryAsync(summary);          // Resumen Diario
await service.SendVoidedAsync(voided);            // Comunicacion de Baja
// Percepcion y Retencion: generar XML + enviar manualmente
```

---

## SOAP vs GRE REST API

Khipu.Net soporta ambos protocolos de SUNAT. Ambos implementan `ISunatClient` y son intercambiables.

### SOAP (legacy)

```csharp
var client = new SunatSoapClient("20123456789MODDATOS", "clavesol", SunatEndpoints.Beta);
var service = new SunatService(client, signer);
```

### GRE REST API (moderno)

SUNAT esta migrando hacia esta API REST con OAuth2. Usa endpoints separados para autenticacion y envio de CPE.

```csharp
var client = new GreClient(
    clientId:     "mi-client-id",
    clientSecret: "mi-client-secret",
    ruc:          "20123456789",
    solUser:      "MODDATOS",
    solPassword:  "clavesol"
);
var service = new SunatService(client, signer);
```

| | `SunatSoapClient` | `GreClient` |
|---|---|---|
| **Protocolo** | SOAP/XML | REST/JSON |
| **Auth** | HTTP Basic | OAuth2 Bearer (JWT) |
| **Token** | No | Auto-refresh con cache thread-safe |
| **Envio** | SOAP envelope + ZIP base64 | POST JSON + ZIP base64 + SHA256 |
| **Auth endpoint** | N/A | `api-seguridad.sunat.gob.pe/v1` |
| **CPE endpoint** | `e-factura.sunat.gob.pe/.../billService` | `api-cpe.sunat.gob.pe/v1` |
| **Consulta CDR** | `getStatusCdr` directo | Via ticket con `getStatus` |
| **Estado** | Estable, legacy | Moderno, recomendado por SUNAT |

---

## Generacion XML

### Builders (documento a XML)

```csharp
using Khipu.Xml.Builder;

var builder = new InvoiceXmlBuilder();
string xml = builder.Build(invoice);
string fileName = builder.GetFileName(invoice);
// "20123456789-01-F001-00000001.xml"
```

9 builders: `InvoiceXmlBuilder`, `ReceiptXmlBuilder`, `CreditNoteXmlBuilder`, `DebitNoteXmlBuilder`, `DespatchXmlBuilder`, `PerceptionXmlBuilder`, `RetentionXmlBuilder`, `SummaryXmlBuilder`, `VoidedXmlBuilder`.

### Parsers (XML a documento)

```csharp
using Khipu.Xml.Parser;

Invoice invoice       = XmlDocumentParser.ParseInvoice(xml);
CreditNote note       = XmlDocumentParser.ParseCreditNote(xml);
Despatch despatch     = XmlDocumentParser.ParseDespatch(xml);
Perception perception = XmlDocumentParser.ParsePerception(xml);
Retention retention   = XmlDocumentParser.ParseRetention(xml);
Summary summary       = XmlDocumentParser.ParseSummary(xml);
Voided voided         = XmlDocumentParser.ParseVoided(xml);
```

---

## CDR y codigos de error

### Parsear respuesta CDR

```csharp
using Khipu.Ws.Reader;

var cdr = CdrReader.ParseFromZip(response.CdrZip);

if (cdr.IsAccepted)  // codigo 0 o >= 4000
    Console.WriteLine($"Aceptado: {cdr.Description}");
else
    Console.WriteLine($"Rechazado [{cdr.Code}]: {cdr.Description}");
```

### 1710 codigos de error SUNAT

```csharp
using Khipu.Ws.Constants;

string msg    = SunatErrorCodes.GetMessage("2017");
bool accepted = SunatErrorCodes.IsAccepted("4001");   // true
bool rejected = SunatErrorCodes.IsRejection("2017");   // true
string cat    = SunatErrorCodes.GetCategory("0306");    // "XML/Parsing"
int total     = SunatErrorCodes.Count;                  // 1710
```

---

## Reportes e impresion

### HTML

```csharp
using Khipu.Core.Report;

var report = new HtmlReport();
string html = report.RenderInvoice(invoice, new ReportParameters
{
    LogoBase64 = Convert.ToBase64String(File.ReadAllBytes("logo.png")),
    Hash = signatureHash
});
```

9 metodos: `RenderInvoice`, `RenderReceipt`, `RenderCreditNote`, `RenderDebitNote`, `RenderDespatch`, `RenderPerception`, `RenderRetention`, `RenderSummary`, `RenderVoided`.

### PDF (requiere wkhtmltopdf)

```csharp
byte[] pdf = await PdfExporter.HtmlToPdfAsync(html);
await PdfExporter.SavePdfAsync(html, "factura.pdf");
```

### QR Code (formato SUNAT)

```csharp
string svg     = QrGenerator.GenerateInvoiceQrSvg(invoice, hash);
string dataUri = QrGenerator.GenerateInvoiceQrBase64(invoice);
byte[] png     = QrGenerator.GenerateQrPng("contenido");
```

---

## Validacion

### Engine por tipo de documento

```csharp
using Khipu.Validator.Engine;

var engine = new DocumentValidationEngine();
var result = engine.ValidateInvoice(invoice);

if (!result.IsValid)
    foreach (var e in result.Errors)
        Console.WriteLine($"[{e.Code}] {e.Path}: {e.Message}");
```

Valida: Invoice, CreditNote, DebitNote, Despatch, Perception, Retention, Summary, Voided.

### Validacion profunda por campo

```csharp
using Khipu.Validator.Rules;

var errors = FieldValidators.ValidateInvoiceDeep(invoice);
var errors = FieldValidators.ValidateDespatchDeep(despatch);
var errors = FieldValidators.ValidatePerceptionDeep(perception);
```

### Constraint loaders (paridad Greenter/Symfony)

```csharp
var errors = ConstraintLoaders.LoadInvoice(invoice);
var errors = ConstraintLoaders.LoadDespatch(despatch);
var errors = ConstraintLoaders.LoadClient(client);
var errors = ConstraintLoaders.LoadCompany(company);
```

33 loaders: Company, Client, Address, Invoice, CreditNote, DebitNote, SaleDetail, Despatch, DespatchDetail, Direction, Transportist, Perception, PerceptionDetail, Retention, RetentionDetail, Summary, SummaryDetail, Voided, VoidedDetail, Cuota, Detraction, Document, Legend, Prepayment, Payment, SalePerception, SummaryPerception, FormaPagoContado, FormaPagoCredito, Charge.

---

## Calculos de impuestos

```csharp
using Khipu.Core.Algorithms;

decimal igv        = TaxCalculator.CalculateIgv(1000);        // 180
decimal precio     = TaxCalculator.CalculateSalePrice(1000, TaxType.Gravado);  // 1180
decimal detraccion = TaxCalculator.CalculateDetraction(1000); // 100
decimal valor      = TaxCalculator.CalculateUnitValue(1180, TaxType.Gravado);  // 1000

// Monto en letras
string letras = AmountInWordsEsPe.Convert(1180.50m, Currency.Pen);
// "SON: MIL CIENTO OCHENTA CON 50/100 SOLES"

// Redondeo SUNAT (2 decimales, AwayFromZero)
decimal rounded = RoundingPolicy.RoundSunat(10.005m); // 10.01
```

---

## Datos de prueba

```csharp
using Khipu.Data.Generators;

var invoice    = DocumentGenerator.CreateInvoice();
var receipt    = DocumentGenerator.CreateReceipt();
var creditNote = DocumentGenerator.CreateCreditNote();
var debitNote  = DocumentGenerator.CreateDebitNote();
var despatch   = DocumentGenerator.CreateDespatch();
var perception = DocumentGenerator.CreatePerception();
var retention  = DocumentGenerator.CreateRetention();
var summary    = DocumentGenerator.CreateSummary();
var voided     = DocumentGenerator.CreateVoided();
```

---

## Paridad con Greenter PHP

| Componente | Greenter PHP | Khipu.Net | Paridad |
|:-----------|:-------------|:----------|:-------:|
| Modelos (9 tipos) | 286 archivos | 87 archivos | 100% |
| XML Builders | 8 (Twig) | 9 (XDocument) | 100% |
| XML Parsers | 8 | 8 | 100% |
| CDR Parser | DomCdrReader | CdrReader | 100% |
| Error Codes | 1710 (XML) | 1710 (embedded) | 100% |
| SOAP Client | BillSender | SunatSoapClient | 100% |
| GRE REST API | Api + GreSender | GreClient | 100% |
| Firma Digital | XMLSecLibs | System.Security | 100% |
| Tax Calculator | Implicito | Explicito | 100% |
| Validacion | 43 Symfony loaders | 33 loaders + field | 100% |
| HTML Reports | 6 Twig templates | 9 metodos | 100% |
| HTML to PDF | phpwkhtmltopdf | PdfExporter | 100% |
| QR Code | BaconQrCode | QRCoder | 100% |
| Data Generators | 19 archivos | 9 factory methods | 100% |
| Tests | PHPUnit | **409 xUnit** | 100% |

---

## Tests

```bash
dotnet test
# 409 tests passing, 0 errors, 0 warnings
```

---

## Documentacion

| Guia | Descripcion |
|------|-------------|
| [Getting Started](docs/getting-started.md) | Instalacion, configuracion, primer envio |
| [Invoices](docs/invoices.md) | Facturas, boletas, Factory y Builder patterns |
| [Credit/Debit Notes](docs/credit-debit-notes.md) | Notas de credito y debito con catalogos SUNAT |
| [Summaries](docs/summaries.md) | Resumenes diarios, comunicaciones de baja, polling CDR |
| [Tax Calculator](docs/tax-calculator.md) | IGV, ISC, IVAP, detracciones, monto en letras |
| [Signing & Sending](docs/signing-and-sending.md) | Firma X.509, SOAP, GRE REST, endpoints |
| [Validation](docs/validation.md) | Engine, field validators, constraint loaders |
| [API Reference](docs/api-reference.md) | Referencia completa de namespaces y clases |

---

## LLM / AI Context

Este proyecto soporta el estandar [llms.txt](https://llmstxt.org/):

- [`llms.txt`](llms.txt) - Indice para LLMs
- [`llms-full.txt`](llms-full.txt) - Documentacion completa inline

---

## Licencia

MIT

## Creditos

- [Greenter PHP](https://github.com/thegreenter/greenter) - Proyecto original de [Giansalex](https://github.com/giansalex)

<p align="center"><sub>Khipu - nombrado en honor al antiguo sistema de contabilidad inca</sub></p>
