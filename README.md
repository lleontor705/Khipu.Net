# Khipu.Net 🪢

Librería .NET Core 10 para Facturación Electrónica SUNAT Perú

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-44%20passing-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

## Descripción

Los **khipus** eran sistemas de contabilidad utilizados por los incas, consistían en cuerdas con nudos para registrar información numérica y administrativa.

**Khipu.Net** es una implementación moderna en .NET Core 10 para facturación electrónica en Perú, inspirada en la popular librería [Greenter](https://github.com/thegreenter/greenter).

## Estado del Proyecto

🎉 **Versión Alpha - Funcional**

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| **Khipu.Data** | ✅ 100% | Modelos y entidades completos |
| **Khipu.Core** | ✅ 100% | Builders, validación, firma digital |
| **Khipu.Xml** | ✅ 100% | Generación XML UBL 2.1 completa |
| **Khipu.Ws** | 🔄 50% | Cliente SOAP SUNAT (stub) |

## Características

- 🔤 Modelos completos para todos los comprobantes SUNAT
- 🏗️ Builder pattern con fluent API
- ✅ Validación de RUC/DNI
- 📊 Cálculo automático de impuestos (IGV, ISC, IVAP)
- 📄 Generación XML UBL 2.1 para todos los documentos
- 🔏 Firma digital XMLDSig con certificados X.509
- 🌐 Cliente SOAP SUNAT (beta)
- 🧪 44+ tests unitarios

## Documentos Soportados

| Documento | Código | Modelo | XML Builder | Firma Digital |
|-----------|--------|--------|-------------|---------------|
| Factura | 01 | ✅ | ✅ | ✅ |
| Boleta de Venta | 03 | ✅ | ✅ | ✅ |
| Nota de Crédito | 07 | ✅ | ✅ | ✅ |
| Nota de Débito | 08 | ✅ | ✅ | ✅ |
| Guía de Remisión | 09 | ✅ | ⏳ | ✅ |
| Comprobante de Retención | 20 | ✅ | ⏳ | ✅ |
| Comprobante de Percepción | 40 | ✅ | ⏳ | ✅ |
| Resumen de Boletas | RC | ✅ | ✅ | ✅ |
| Comunicación de Bajas | RA | ✅ | ✅ | ✅ |

## Instalación

`ash
dotnet add package Khipu.Net
`

## Uso Rápido

### 1. Crear Factura

`csharp
using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

var invoice = new InvoiceBuilder()
    .WithCompany(new Company 
    { 
        Ruc = \"20123456789\", 
        RazonSocial = \"MI EMPRESA SAC\",
        Address = new Address
        {
            Ubigeo = \"150101\",
            Departamento = \"LIMA\",
            Provincia = \"LIMA\",
            Distrito = \"LIMA\",
            Direccion = \"AV. PRINCIPAL 123\"
        }
    })
    .WithClient(new Client 
    { 
        TipoDoc = DocumentType.Ruc, 
        NumDoc = \"20987654321\", 
        RznSocial = \"CLIENTE SRL\" 
    })
    .WithSerie(\"F001\")
    .WithCorrelativo(123)
    .WithFechaEmision(DateTime.Now)
    .AddDetail(new SaleDetail
    {
        Codigo = \"PROD001\",
        Descripcion = \"Producto de prueba\",
        Cantidad = 2,
        MtoValorUnitario = 100,
        MtoValorVenta = 200
    })
    .Build();
`

### 2. Generar XML UBL 2.1

#### Para Factura
`csharp
using Khipu.Xml.Builder;

var xmlBuilder = new InvoiceXmlBuilder();
var xml = xmlBuilder.Build(invoice);
var fileName = xmlBuilder.GetFileName(invoice);
// fileName: \"20123456789-01-F001-00000123.xml\"
`

#### Para Resumen de Boletas
`csharp
var summaryBuilder = new SummaryXmlBuilder();
var xml = summaryBuilder.Build(summary);
var fileName = summaryBuilder.GetFileName(summary);
// fileName: \"20123456789-RC-20260326-001.xml\"
`

### 3. Firmar XML con Certificado Digital

`csharp
using Khipu.Core.Security;

// Cargar certificado PFX
var signer = XmlSigner.FromPfx(\"certificado.pfx\", \"password\");

// Firmar XML
var signedXml = signer.Sign(xml);
`

### 4. Enviar a SUNAT

`csharp
using Khipu.Ws.Services;
using Khipu.Ws.Helpers;

// Crear cliente SOAP
var client = new SunatSoapClient(\"20123456789MODDATOS\", \"password\", \"https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService\");

// Crear ZIP
var zipContent = ZipHelper.CreateZip(fileName, signedXml);

// Enviar
var response = await client.SendBillAsync(zipContent, fileName);

if (response.Success)
{
    Console.WriteLine($\"Comprobante enviado exitosamente\");
    Console.WriteLine($\"CDR recibido: {response.CdrZip?.Length} bytes\");
}
`

## Arquitectura

`
Khipu.Net/
├── src/
│   ├── Khipu.Core/         # Lógica de negocio
│   │   ├── Builder/        # InvoiceBuilder, ReceiptBuilder
│   │   ├── Constants/      # SunatConstants (IGV, ISC, IVAP)
│   │   ├── Validation/     # DocumentValidator (RUC/DNI)
│   │   └── Security/       # XmlSigner (Firma digital)
│   ├── Khipu.Data/         # Modelos y entidades
│   │   ├── Documents/      # Invoice, Receipt, CreditNote, etc.
│   │   ├── Entities/       # Company, Client, Address
│   │   ├── Common/         # Legend, Detraction, Prepayment
│   │   └── Enums/          # VoucherType, Currency, TaxType
│   ├── Khipu.Xml/          # Generación XML UBL 2.1
│   │   └── Builder/        # Todos los XML builders
│   ├── Khipu.Ws/           # Cliente SOAP SUNAT
│   │   ├── Services/       # SunatSoapClient
│   │   ├── Models/         # Responses
│   │   ├── Helpers/        # ZipHelper
│   │   └── Constants/      # SunatEndpoints
│   └── Khipu.Validator/    # Validaciones avanzadas (TODO)
└── tests/
    └── Khipu.Tests/        # Tests unitarios (44 passing)
`

## Ejemplo Completo

`csharp
using Khipu.Core.Builder;
using Khipu.Core.Security;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Khipu.Ws.Services;
using Khipu.Ws.Helpers;

// 1. Crear factura
var invoice = new InvoiceBuilder()
    .WithCompany(new Company { Ruc = \"20123456789\", RazonSocial = \"EMPRESA\" })
    .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENTE\" })
    .WithSerie(\"F001\")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Now)
    .AddDetail(new SaleDetail
    {
        Codigo = \"PROD001\",
        Descripcion = \"Producto\",
        Cantidad = 1,
        MtoValorUnitario = 100,
        MtoValorVenta = 100,
        PrecioVenta = 118
    })
    .Build();

// 2. Validar
if (new InvoiceBuilder().Validate())
{
    // 3. Generar XML
    var xmlBuilder = new InvoiceXmlBuilder();
    var xml = xmlBuilder.Build(invoice);
    
    // 4. Firmar
    var signer = XmlSigner.FromPfx(\"certificado.pfx\", \"password\");
    var signedXml = signer.Sign(xml);
    
    // 5. Enviar a SUNAT
    var client = new SunatSoapClient(\"20123456789MODDATOS\", \"password\", SunatEndpoints.Beta);
    var zipContent = ZipHelper.CreateZip(xmlBuilder.GetFileName(invoice), signedXml);
    var response = await client.SendBillAsync(zipContent, xmlBuilder.GetFileName(invoice));
    
    if (response.Success)
    {
        Console.WriteLine(\"✅ Comprobante enviado exitosamente!\");
    }
}
`

## Roadmap

- [x] Modelos de datos
- [x] Builder pattern
- [x] Validación RUC/DNI
- [x] Cálculo de impuestos
- [x] Generación XML UBL 2.1 (todos los documentos)
- [x] Firma digital X.509
- [x] Cliente SOAP SUNAT (beta)
- [ ] Parsing completo de respuestas SOAP
- [ ] Manejo de errores SUNAT
- [ ] PDF generation
- [ ] NuGet package

## Tests

`ash
dotnet test
# 44 tests passing
`

## Contribuir

Las contribuciones son bienvenidas. Por favor, lee las guías de contribución.

## Licencia

MIT

## Inspiración

- [Greenter PHP](https://github.com/thegreenter/greenter) por Giansalex

---

*Nombrado en honor al antiguo sistema de contabilidad inca* 🇵🇪
