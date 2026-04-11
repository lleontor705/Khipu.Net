# API Reference

## Khipu.Data

### Documents

| Class | Description | Base |
|-------|-------------|------|
| `Invoice` | Factura | BaseSale |
| `Receipt` | Boleta de venta | BaseSale |
| `CreditNote` | Nota de credito | BaseSale |
| `DebitNote` | Nota de debito | BaseSale |
| `Despatch` | Guia de remision | - |
| `Summary` | Resumen diario | - |
| `Voided` | Comunicacion de bajas | - |
| `Reversion` | Reversion de documentos | Voided |
| `Retention` | Comprobante de retencion | - |
| `Perception` | Comprobante de percepcion | - |

### Entities

| Class | Key properties |
|-------|---------------|
| `Company` | Ruc, RazonSocial, NombreComercial, Address, Email, Telephone |
| `Client` | TipoDoc, NumDoc, RznSocial, Address, Email, Telephone |
| `Address` | Ubigeo, Departamento, Provincia, Distrito, Direccion, CodigoLocal, CodigoPais |
| `SaleDetail` | Codigo, Descripcion, Unidad, Cantidad, MtoValorUnitario, TipoAfectacionIgv, Atributos |
| `DespatchDetail` | Codigo, Descripcion, Unidad, Cantidad, CodProdSunat, Atributos |

### Common

| Class | Key properties |
|-------|---------------|
| `Legend` | Code, Value |
| `Charge` | CodTipo, Factor, Monto, MontoBase |
| `Cuota` | Moneda, Monto, FechaPago |
| `Detraction` | Monto, CtaBanco, CodBienDetraccion, Porcentaje, ValueRef |
| `PaymentTerms` | Tipo, Moneda, Monto |
| `Prepayment` | Nro, TipoDoc, NroDoc, Total |
| `Document` | TipoDoc, NroDoc |
| `AdditionalDoc` | TipoDesc, Tipo, Nro, Emisor |
| `DetailAttribute` | Code, Name, Value, FecInicio, FecFin, Duracion |
| `Exchange` | MonedaRef, MonedaObj, Factor, Fecha |
| `Transportist` | TipoDoc, NumDoc, RznSocial, Placa |
| `Vehicle` | Placa, NroCirculacion, NroPlacaRemolque |
| `Driver` | TipoDoc, NumDoc, Nombres, Apellidos, Licencia |
| `Payment` | FormaPago, Monto, NumOperacion |

### Enums

| Enum | Values |
|------|--------|
| `DocumentType` | Ruc(6), Dni(1), CarnetExtranjeria(4), Pasaporte(7), SinRuc(0) |
| `VoucherType` | Factura(1), Boleta(3), NotaCredito(7), NotaDebito(8), GuiaRemision(9), Retencion(20), Percepcion(40) |
| `Currency` | Pen(1), Usd(2), Eur(3) |
| `TaxType` | Gravado(10), Exonerado(20), Inafecto(30), Exportacion(40), Gratuito(21), Ivap(17) |

### Generators

```csharp
DocumentGenerator.CreateInvoice(correlativo)
DocumentGenerator.CreateReceipt(correlativo)
DocumentGenerator.CreateCreditNote(correlativo)
DocumentGenerator.CreateDebitNote(correlativo)
DocumentGenerator.CreateDespatch(correlativo)
DocumentGenerator.CreatePerception(correlativo)
DocumentGenerator.CreateRetention(correlativo)
DocumentGenerator.CreateSummary()
DocumentGenerator.CreateVoided()
```

---

## Khipu.Core

### InvoiceBuilder

```csharp
WithCompany(Company) → WithClient(Client) → WithSerie(string) → WithCorrelativo(int)
→ WithFechaEmision(DateTime) → AddDetail(SaleDetail) → Build() → Invoice
```

### InvoiceFactory

```csharp
new InvoiceFactory(Company)
  .CreateInvoice(Client, serie, correlativo, fecha)
  .CreateReceipt(Client, serie, correlativo, fecha)
  .CreateCreditNote(Client, serie, correlativo, fecha, docAfectado, codMotivo, desMotivo)
  .CreateDebitNote(Client, serie, correlativo, fecha, docAfectado, codMotivo, desMotivo)
  .CreateDetail(codigo, descripcion, unidad, cantidad, valorUnitario, tipoAfectacion)
```

### TaxCalculator

```csharp
TaxCalculator.CalculateIgv(decimal taxableAmount) → decimal
TaxCalculator.CalculateSalePrice(decimal base, TaxType type) → decimal
TaxCalculator.CalculateUnitValue(decimal salePrice, TaxType type) → decimal
TaxCalculator.CalculateDetraction(decimal base, decimal rate = 0.10m) → decimal
```

### AmountInWordsEsPe

```csharp
AmountInWordsEsPe.Convert(1180.50m, Currency.Pen)
// → "SON: MIL CIENTO OCHENTA CON 50/100 SOLES"
```

### XmlSigner

```csharp
XmlSigner.FromPfx(string pfxPath, string password) → XmlSigner
signer.Sign(string xmlContent) → string
signer.IsCertificateValid() → bool
signer.GetCertificateInfo() → CertificateInfo
```

### SunatService

```csharp
new SunatService(string user, string pass, string endpoint, XmlSigner? signer)
new SunatService(ISunatClient client, XmlSigner? signer)

SendInvoiceAsync(Invoice) → Task<SunatResponse>
SendReceiptAsync(Receipt) → Task<SunatResponse>
SendCreditNoteAsync(CreditNote) → Task<SunatResponse>
SendDebitNoteAsync(DebitNote) → Task<SunatResponse>
SendDespatchAsync(Despatch) → Task<SunatResponse>
SendSummaryAsync(Summary) → Task<SunatResponse>
SendVoidedAsync(Voided) → Task<SunatResponse>
GetStatusAsync(string ticket) → Task<TicketResponse>
GetCdrAsync(ruc, tipo, serie, corr) → Task<CdrResponse>
SendSummaryAndQueryCdrAsync(...) → Task<CdrResponse>
```

### HtmlReport

```csharp
RenderInvoice(Invoice, ReportParameters?) → string
RenderReceipt(Receipt, ReportParameters?) → string
RenderCreditNote(CreditNote, ReportParameters?) → string
RenderDebitNote(DebitNote, ReportParameters?) → string
RenderDespatch(Despatch, ReportParameters?) → string
RenderPerception(Perception, ReportParameters?) → string
RenderRetention(Retention, ReportParameters?) → string
RenderSummary(Summary, ReportParameters?) → string
RenderVoided(Voided, ReportParameters?) → string
```

### QrGenerator

```csharp
QrGenerator.GenerateQrSvg(string content) → string
QrGenerator.GenerateQrBase64(string content) → string (data URI)
QrGenerator.GenerateQrPng(string content) → byte[]
QrGenerator.GenerateInvoiceQrSvg(Invoice, hash?) → string
QrGenerator.GenerateInvoiceQrBase64(Invoice, hash?) → string
QrGenerator.GenerateDespatchQrSvg(Despatch) → string
QrGenerator.GetInvoiceQrContent(Invoice, hash?) → string
QrGenerator.GetReceiptQrContent(Receipt, hash?) → string
QrGenerator.GetDespatchQrContent(Despatch) → string
```

### PdfExporter

```csharp
PdfExporter.HtmlToPdfAsync(string html, PdfOptions?) → Task<byte[]>
PdfExporter.SavePdfAsync(string html, string path, PdfOptions?) → Task
```

---

## Khipu.Xml

### Builders (IXmlBuilder&lt;T&gt;)

| Builder | Type | Root element |
|---------|------|-------------|
| `InvoiceXmlBuilder` | Invoice | Invoice |
| `ReceiptXmlBuilder` | Receipt | Invoice |
| `CreditNoteXmlBuilder` | CreditNote | CreditNote |
| `DebitNoteXmlBuilder` | DebitNote | DebitNote |
| `DespatchXmlBuilder` | Despatch | DespatchAdvice |
| `PerceptionXmlBuilder` | Perception | Perception |
| `RetentionXmlBuilder` | Retention | Retention |
| `SummaryXmlBuilder` | Summary | SummaryDocuments |
| `VoidedXmlBuilder` | Voided | VoidedDocuments |

### Parser

```csharp
XmlDocumentParser.ParseInvoice(string xml) → Invoice?
XmlDocumentParser.ParseCreditNote(string xml) → CreditNote?
XmlDocumentParser.ParseDebitNote(string xml) → DebitNote?
XmlDocumentParser.ParseDespatch(string xml) → Despatch?
XmlDocumentParser.ParsePerception(string xml) → Perception?
XmlDocumentParser.ParseRetention(string xml) → Retention?
XmlDocumentParser.ParseSummary(string xml) → Summary?
XmlDocumentParser.ParseVoided(string xml) → Voided?
```

---

## Khipu.Ws

### ISunatClient (interface)

```csharp
SendBillAsync(SunatSendRequest, CancellationToken) → Task<SunatResponse>
SendSummaryAsync(SunatSendRequest, CancellationToken) → Task<SunatResponse>
GetStatusAsync(string ticket, CancellationToken) → Task<TicketResponse>
GetCdrAsync(CdrQuery, CancellationToken) → Task<CdrResponse>
```

Implementations: `SunatSoapClient` (SOAP), `GreClient` (REST).

### GreClient

```csharp
new GreClient(clientId, clientSecret, ruc, solUser, solPassword,
    authEndpoint?, cpeEndpoint?, httpClient?)
```

### CdrReader

```csharp
CdrReader.Parse(string? xml) → CdrDetail?
CdrReader.ParseFromZip(byte[]? zipData) → CdrDetail?
```

### SunatErrorCodes

```csharp
SunatErrorCodes.GetMessage(string? code) → string?
SunatErrorCodes.GetMessageOrDefault(string? code, string default) → string
SunatErrorCodes.IsAccepted(string? code) → bool    // 0 or >= 4000
SunatErrorCodes.IsObservation(string? code) → bool  // >= 4000
SunatErrorCodes.IsRejection(string? code) → bool    // 1-3999
SunatErrorCodes.GetCategory(string? code) → string
SunatErrorCodes.GetAll() → IReadOnlyDictionary<string, string>
SunatErrorCodes.Count → int  // 1710
```

### Response models

| Class | Properties |
|-------|-----------|
| `SunatResponse` | Success, Ticket, CdrZip, ErrorCode, ErrorMessage, StatusCode |
| `TicketResponse` | Success, Ticket, CdrZip, StatusCode, ErrorMessage |
| `CdrResponse` | Success, CdrZip, CdrXml, IsAccepted, ErrorCode, ErrorMessage |
| `CdrDetail` | Id, Code, Description, Notes, Reference, IsAccepted |

---

## Khipu.Validator

### DocumentValidationEngine

```csharp
ValidateInvoice(Invoice) → ValidationResult
ValidateCreditNote(CreditNote) → ValidationResult
ValidateDebitNote(DebitNote) → ValidationResult
ValidateDespatch(Despatch) → ValidationResult
ValidatePerception(Perception) → ValidationResult
ValidateRetention(Retention) → ValidationResult
ValidateSummary(Summary) → ValidationResult
ValidateVoided(Voided) → ValidationResult
```

### FieldValidators (deep validation)

```csharp
ValidateInvoiceDeep(Invoice) → List<ValidationError>
ValidateDespatchDeep(Despatch) → List<ValidationError>
ValidatePerceptionDeep(Perception) → List<ValidationError>
ValidateRetentionDeep(Retention) → List<ValidationError>
ValidateSummaryDeep(Summary) → List<ValidationError>
ValidateClient(Client?, prefix) → List<ValidationError>
ValidateCompany(Company?, prefix) → List<ValidationError>
ValidateAddress(Address?, prefix) → List<ValidationError>
ValidateSaleDetail(SaleDetail?, lineNumber, prefix) → List<ValidationError>
```

### ConstraintLoaders (33 loaders)

```csharp
LoadInvoice, LoadCreditNote, LoadDebitNote, LoadDespatch, LoadPerception,
LoadRetention, LoadSummary, LoadVoided, LoadCompany, LoadClient, LoadAddress,
LoadSaleDetail, LoadDespatchDetail, LoadDirection, LoadTransportist,
LoadPerceptionDetail, LoadRetentionDetail, LoadSummaryDetail, LoadVoidedDetail,
LoadCuota, LoadDetraction, LoadDocument, LoadLegend, LoadPrepayment,
LoadPayment, LoadSalePerception, LoadSummaryPerception,
LoadFormaPagoContado, LoadFormaPagoCredito, LoadCharge
```

### ValidationResult

```csharp
record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
record ValidationError(string Code, string Path, string Message, string Severity = "Error")
```
