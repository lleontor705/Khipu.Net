# Signing and Sending Documents

## Digital signature

SUNAT requires all electronic documents to be digitally signed using XMLDSig with an X.509 certificate.

### Loading a certificate

```csharp
using Khipu.Core.Security;

// From PFX file (recommended)
var signer = XmlSigner.FromPfx("certificate.pfx", "password");

// From X509Certificate2 object
var cert = new X509Certificate2("certificate.pfx", "password");
var signer = new XmlSigner(cert);
```

### Certificate validation

```csharp
// Check if certificate is valid
if (!signer.IsCertificateValid())
{
    var info = signer.GetCertificateInfo();
    Console.WriteLine($"Subject: {info.Subject}");
    Console.WriteLine($"Expires: {info.NotAfter}");
    Console.WriteLine($"Has key: {info.HasPrivateKey}");
}
```

### Manual XML signing

```csharp
string signedXml = signer.Sign(xmlContent);
```

## Sending documents to SUNAT

### SunatService

`SunatService` handles the full flow: XML generation, signing, zipping, and SOAP submission.

```csharp
using Khipu.Core.Services;

var service = new SunatService(
    username: "20123456789MODDATOS",
    password: "moddatos",
    endpoint: SunatEndpoints.Beta,
    signer: signer
);
```

### Sending invoices and notes

```csharp
// Invoice
var response = await service.SendInvoiceAsync(invoice);

// Credit note
var response = await service.SendCreditNoteAsync(creditNote);

// Summary (boletas)
var response = await service.SendSummaryAsync(summary);

// Voided
var response = await service.SendVoidedAsync(voided);
```

### Handling responses

```csharp
if (response.Success)
{
    Console.WriteLine("Document accepted");

    if (response.CdrZip != null)
        File.WriteAllBytes("cdr.zip", response.CdrZip);
}
else
{
    Console.WriteLine($"Error {response.ErrorCode}: {response.ErrorMessage}");
}
```

### Retrieving CDR

```csharp
var cdr = await service.GetCdrAsync(
    ruc: "20123456789",
    tipoComprobante: "01",
    serie: "F001",
    correlativo: 1
);

if (cdr.IsAccepted)
    Console.WriteLine("Document accepted by SUNAT");
```

## Using the SOAP client directly

For advanced scenarios, use `SunatSoapClient` directly:

```csharp
using Khipu.Ws.Services;
using Khipu.Ws.Models;

var client = new SunatSoapClient(
    username: "20123456789MODDATOS",
    password: "moddatos",
    endpoint: "https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService"
);

var request = new SunatSendRequest(
    ZipContent: zipBytes,
    FileNameWithoutExtension: "20123456789-01-F001-00000001"
);

var response = await client.SendBillAsync(request);
```

## Document numbering

```csharp
using Khipu.Core.Services;

var numbering = new DocumentNumberService(initialCorrelativo: 1);

int next = numbering.GetNextCorrelativo();  // 1, 2, 3...

string docNumber = numbering.GenerateDocumentNumber("F001", 1);
// "F001-00000001"

string fileName = numbering.GenerateFileName("20123456789", "01", "F001", 1);
// "20123456789-01-F001-00000001.xml"

string zipName = numbering.GenerateZipName("20123456789", "01", "F001", 1);
// "20123456789-01-F001-00000001.zip"
```
