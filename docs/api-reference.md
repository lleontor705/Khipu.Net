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
| `Summary` | Resumen diario de boletas | - |
| `Voided` | Comunicacion de bajas | - |
| `Retention` | Comprobante de retencion | - |
| `Perception` | Comprobante de percepcion | - |

### Entities

| Class | Key properties |
|-------|---------------|
| `Company` | Ruc, RazonSocial, NombreComercial, Address |
| `Client` | TipoDoc, NumDoc, RznSocial, Address |
| `Address` | Ubigeo, Departamento, Provincia, Distrito, Direccion |
| `SaleDetail` | Codigo, Descripcion, Unidad, Cantidad, MtoValorUnitario, TipoAfectacionIgv |
| `Legend` | Code, Value |
| `Charge` | CodTipo, Factor, Monto, MontoBase |
| `Cuota` | Moneda, Monto, FechaPago |
| `Detraction` | Mount, CtaBanco, CodBienDetraccion, Porcentaje |
| `PaymentTerms` | Tipo (Contado/Credito), Moneda, Monto |
| `Prepayment` | Nro, TipoDoc, NroDoc, Total |

### Enums

| Enum | Values |
|------|--------|
| `DocumentType` | Ruc(6), Dni(1), CarnetExtranjeria(4), Pasaporte(7), SinRuc(0) |
| `VoucherType` | Factura(1), Boleta(3), NotaCredito(7), NotaDebito(8), GuiaRemision(9), Retencion(20), Percepcion(40) |
| `Currency` | Pen(1), Usd(2), Eur(3) |
| `TaxType` | Gravado(10), Exonerado(20), Inafecto(30), Exportacion(40), Gratuito(21), Ivap(17) |

---

## Khipu.Core

### InvoiceFactory

```csharp
public InvoiceFactory(Company company)

public Invoice CreateInvoice(Client client, string serie, int correlativo, DateTime fechaEmision)
public Receipt CreateReceipt(Client client, string serie, int correlativo, DateTime fechaEmision)
public CreditNote CreateCreditNote(Client client, string serie, int correlativo,
    DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
public DebitNote CreateDebitNote(Client client, string serie, int correlativo,
    DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
public SaleDetail CreateDetail(string codigo, string descripcion, string unidad,
    decimal cantidad, decimal valorUnitario, TaxType tipoAfectacion = TaxType.Gravado)
```

### InvoiceBuilder

```csharp
public IInvoiceBuilder WithCompany(Company company)
public IInvoiceBuilder WithClient(Client client)
public IInvoiceBuilder WithSerie(string serie)
public IInvoiceBuilder WithCorrelativo(int correlativo)
public IInvoiceBuilder WithFechaEmision(DateTime fecha)
public IInvoiceBuilder AddDetail(SaleDetail detail)
public Invoice Build()
public bool Validate()
public List<string> GetErrors()
```

### TaxCalculator

```csharp
public static decimal CalculateIgv(decimal taxableAmount)
public static decimal CalculateSalePrice(decimal baseAmount, TaxType taxType)
public static decimal CalculateUnitValue(decimal salePrice, TaxType taxType)
public static decimal CalculateDetraction(decimal baseAmount, decimal rate = 0.10m)
```

### RoundingPolicy

```csharp
public static decimal RoundSunat(decimal value)
    // 2 decimal places, MidpointRounding.AwayFromZero
```

### AmountInWordsEsPe

```csharp
public static string Convert(decimal amount, Currency currency)
    // Returns: "SON: [words] CON [cents]/100 [currency]"
```

### XmlSigner

```csharp
public XmlSigner(X509Certificate2 certificate)
public static XmlSigner FromPfx(string pfxPath, string password)
public string Sign(string xmlContent)
public bool IsCertificateValid()
public CertificateInfo GetCertificateInfo()
```

### SunatService

```csharp
public SunatService(string username, string password, string endpoint, XmlSigner? signer = null)

public Task<SunatResponse> SendInvoiceAsync(Invoice invoice)
public Task<SunatResponse> SendCreditNoteAsync(CreditNote note)
public Task<SunatResponse> SendSummaryAsync(Summary summary)
public Task<SunatResponse> SendVoidedAsync(Voided voided)
public Task<TicketResponse> GetStatusAsync(string ticket)
public Task<CdrResponse> GetCdrAsync(string ruc, string tipoComprobante, string serie, int correlativo)
public Task<CdrResponse> SendSummaryAndQueryCdrAsync(Summary summary, string tipoComprobante,
    string serie, int correlativo, int maxStatusChecks = 3, TimeSpan? pollInterval = null)
```

### DocumentNumberService

```csharp
public DocumentNumberService(int initialCorrelativo = 1)

public int GetNextCorrelativo()
public string GenerateDocumentNumber(string serie, int correlativo)
public string GenerateFileName(string ruc, string tipoDoc, string serie, int correlativo)
public string GenerateZipName(string ruc, string tipoDoc, string serie, int correlativo)
```

### DocumentValidator

```csharp
public static bool ValidateRuc(string ruc)
public static bool ValidateDni(string dni)
public static bool ValidateDocument(string tipoDoc, string numDoc)
public static bool ValidateSerie(string serie, string tipoDoc)
public static bool ValidateCorrelativo(int correlativo)
```

---

## Khipu.Xml

### XML Builders

All builders implement `IXmlBuilder<T>`:

```csharp
public interface IXmlBuilder<T>
{
    string Build(T document);        // Returns UBL 2.1 XML
    string GetFileName(T document);  // Returns filename
}
```

| Builder | Document type | XML root element |
|---------|--------------|------------------|
| `InvoiceXmlBuilder` | Invoice, Receipt | `Invoice` |
| `CreditNoteXmlBuilder` | CreditNote | `CreditNote` |
| `DebitNoteXmlBuilder` | DebitNote | `DebitNote` |
| `ReceiptXmlBuilder` | Receipt | `Invoice` |
| `SummaryXmlBuilder` | Summary | `SummaryDocuments` |
| `VoidedXmlBuilder` | Voided | `VoidedDocuments` |

---

## Khipu.Ws

### SunatSoapClient

```csharp
public SunatSoapClient(string username, string password, string endpoint, HttpClient? httpClient = null)

public Task<SunatResponse> SendBillAsync(SunatSendRequest request, CancellationToken ct = default)
public Task<SunatResponse> SendSummaryAsync(SunatSendRequest request, CancellationToken ct = default)
public Task<TicketResponse> GetStatusAsync(string ticket, CancellationToken ct = default)
public Task<CdrResponse> GetCdrAsync(CdrQuery query, CancellationToken ct = default)
```

### Models

```csharp
public sealed record SunatSendRequest(byte[] ZipContent, string FileNameWithoutExtension,
    string? Ruc = null, string? TipoDocumento = null)

public sealed record CdrQuery(string Ruc, string TipoComprobante, string Serie, int Correlativo)
```

| Response | Key properties |
|----------|---------------|
| `SunatResponse` | Success, Ticket, CdrZip, ErrorCode, ErrorMessage |
| `TicketResponse` | Success, Ticket, CdrZip, StatusCode, ErrorMessage |
| `CdrResponse` | Success, CdrZip, CdrXml, IsAccepted, ErrorCode, ErrorMessage |

---

## Khipu.Validator

### DocumentValidationEngine

```csharp
public ValidationResult ValidateInvoice(Invoice invoice)
public ValidationResult ValidateSummary(Summary summary)
public ValidationResult ValidateVoided(Voided voided)
```

### ValidationResult

```csharp
public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
public sealed record ValidationError(string Code, string Path, string Message, string Severity = "Error")
```
