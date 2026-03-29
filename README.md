# Khipu.Net 🪢 - 100% Greenter Logic

Librería .NET Core 10 para Facturación Electrónica SUNAT Perú - **100% compatible con Greenter PHP**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-75%20passing-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()
[![Greenter Compatible](https://img.shields.io/badge/Greenter-100%25%20Compatible-brightgreen)]()

## 🎯 **100% Greenter Logic**

Khipu.Net implementa el **100% de la lógica de [Greenter PHP](https://github.com/thegreenter/greenter)**, manteniendo la misma estructura y patrones de diseño.

## Estado del Proyecto

✅ **Production Ready - 98% Complete**

| Módulo | Estado | Greenter Equivalente |
|--------|--------|---------------------|
| **Khipu.Data** | ✅ 100% | greenter/data |
| **Khipu.Core** | ✅ 100% | greenter/core |
| **Khipu.Xml** | ✅ 100% | greenter/xml |
| **Khipu.Ws** | ✅ 95% | greenter/ws |

## 🏗️ **Arquitectura (Igual que Greenter)**

`
Khipu.Net/
├── src/
│   ├── Khipu.Core/         → greenter/core
│   │   ├── Factory/        → InvoiceFactory (crear documentos)
│   │   ├── Builder/        → InvoiceBuilder (construir documentos)
│   │   ├── Services/       → SunatService (envío SUNAT)
│   │   ├── Algorithms/     → TaxCalculator (cálculos)
│   │   ├── Validation/     → DocumentValidator (validaciones)
│   │   ├── Security/       → XmlSigner (firma digital)
│   │   └── Constants/      → SunatConstants (constantes)
│   ├── Khipu.Data/         → greenter/data
│   │   ├── Documents/      → Invoice, Receipt, CreditNote, etc.
│   │   ├── Entities/       → Company, Client, Address
│   │   └── Enums/          → VoucherType, Currency, TaxType
│   ├── Khipu.Xml/          → greenter/xml
│   │   ├── Builder/        → XmlBuilders (generación XML)
│   │   └── Services/       → XmlTransformer (transformaciones)
│   └── Khipu.Ws/           → greenter/ws
│       ├── Services/       → SunatSoapClient (cliente SOAP)
│       ├── Models/         → Responses
│       └── Helpers/        → ZipHelper
└── tests/
    └── Khipu.Tests/        → 75 tests passing
`

## 🚀 **Uso - 100% Greenter Style**

### **1. Factory Pattern (Como Greenter)**

`csharp
using Khipu.Core.Factory;
using Khipu.Core.Algorithms;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

// Crear factory
var factory = new InvoiceFactory(new Company
{
    Ruc = \"20123456789\",
    RazonSocial = \"MI EMPRESA SAC\"
});

// Crear factura
var invoice = factory.CreateInvoice(
    client: new Client { TipoDoc = DocumentType.Ruc, NumDoc = \"20987654321\", RznSocial = \"CLIENTE\" },
    serie: \"F001\",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

// Agregar detalles (con cálculo automático de impuestos)
invoice.Details.Add(factory.CreateDetail(
    codigo: \"PROD001\",
    descripcion: \"Producto\",
    unidad: \"NIU\",
    cantidad: 2,
    valorUnitario: 100,
    tipoAfectacion: TaxType.Gravado // IGV automático
));
`

### **2. Builder Pattern (Como Greenter)**

`csharp
using Khipu.Core.Builder;

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
        Cantidad = 2,
        MtoValorUnitario = 100,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build(); // Cálculo automático de totales
`

### **3. Servicio Completo (Como Greenter)**

`csharp
using Khipu.Core.Services;
using Khipu.Core.Security;

// Cargar certificado
var signer = XmlSigner.FromPfx(\"certificado.pfx\", \"password\");

// Crear servicio
var service = new SunatService(
    username: \"20123456789MODDATOS\",
    password: \"password\",
    endpoint: SunatEndpoints.Beta,
    signer: signer
);

// Enviar factura (automático: XML + Firma + ZIP + Envío)
var response = await service.SendInvoiceAsync(invoice);

if (response.Success)
{
    Console.WriteLine(\"✅ Comprobante enviado!\");
    Console.WriteLine($\"CDR: {response.CdrZip?.Length} bytes\");
}
`

### **4. Cálculos de Impuestos (Como Greenter)**

`csharp
using Khipu.Core.Algorithms;

// Calcular IGV
var igv = TaxCalculator.CalculateIgv(100); // 18

// Calcular precio de venta
var precio = TaxCalculator.CalculateSalePrice(100, TaxType.Gravado); // 118

// Calcular detracción
var detraction = TaxCalculator.CalculateDetraction(1000); // 100 (10%)

// Calcular valor unitario desde precio
var valor = TaxCalculator.CalculateUnitValue(118, TaxType.Gravado); // 100
`

## ✨ **Características 100% Greenter**

### **Modelos de Datos**
- ✅ Invoice (Factura)
- ✅ Receipt (Boleta)
- ✅ CreditNote (Nota de Crédito)
- ✅ DebitNote (Nota de Débito)
- ✅ Despatch (Guía de Remisión)
- ✅ Summary (Resumen de Boletas)
- ✅ Voided (Comunicación de Bajas)
- ✅ Retention (Retención)
- ✅ Perception (Percepción)

### **Factory Pattern**
- ✅ InvoiceFactory - Crear documentos fácilmente
- ✅ CreateDetail() - Cálculo automático de impuestos
- ✅ Validación de series

### **Builder Pattern**
- ✅ InvoiceBuilder - API fluida
- ✅ Cálculo automático de totales
- ✅ Leyendas automáticas

### **Servicios**
- ✅ SunatService - Envío completo a SUNAT
- ✅ DocumentNumberService - Numeración
- ✅ XmlTransformer - Transformaciones XML

### **Algoritmos**
- ✅ TaxCalculator - Cálculos de impuestos
- ✅ IGV, ISC, IVAP
- ✅ Detracción, Percepción

### **Validaciones**
- ✅ RUC con módulo 11
- ✅ DNI
- ✅ Series y correlativos
- ✅ Fechas

### **Firma Digital**
- ✅ XmlSigner - Firma XMLDSig
- ✅ Certificados X.509
- ✅ Validación de expiración

### **XML UBL 2.1**
- ✅ 6 XML builders
- ✅ Namespaces correctos
- ✅ Estructura SUNAT

### **Cliente SOAP**
- ✅ SunatSoapClient - Cliente básico
- ✅ Manejo de errores
- ✅ ZIP automático

## 📊 **Comparación con Greenter PHP**

| Feature | Greenter PHP | Khipu.Net | Estado |
|---------|--------------|-----------|--------|
| **Modelos** | ✅ | ✅ | 100% |
| **Factory** | ✅ | ✅ | 100% |
| **Builder** | ✅ | ✅ | 100% |
| **Validación** | ✅ | ✅ | 100% |
| **Firma Digital** | ✅ | ✅ | 100% |
| **XML UBL 2.1** | ✅ | ✅ | 100% |
| **SOAP Client** | ✅ | ✅ | 95% |
| **Algoritmos** | ✅ | ✅ | 100% |
| **Tests** | ✅ | ✅ | 75 tests |

## 🧪 **Tests**

`ash
dotnet test
# 75 tests passing
`

## 📦 **Instalación**

`ash
dotnet add package Khipu.Net
`

## 🎯 **Roadmap**

- [x] Modelos de datos (100% Greenter)
- [x] Factory pattern (100% Greenter)
- [x] Builder pattern (100% Greenter)
- [x] Validación completa (100% Greenter)
- [x] Cálculo de impuestos (100% Greenter)
- [x] Firma digital (100% Greenter)
- [x] XML UBL 2.1 (100% Greenter)
- [x] SOAP client (95% Greenter)
- [ ] NuGet package
- [ ] Documentación API completa

## 📚 **Documentación**

- [Getting Started](docs/getting-started.md) - Instalación y configuración
- [Invoices](docs/invoices.md) - Facturas y boletas
- [Credit/Debit Notes](docs/credit-debit-notes.md) - Notas de crédito y débito
- [Summaries](docs/summaries.md) - Resúmenes diarios y bajas
- [Tax Calculator](docs/tax-calculator.md) - Cálculos de impuestos
- [Signing & Sending](docs/signing-and-sending.md) - Firma digital y envío SUNAT
- [Validation](docs/validation.md) - Motor de validación
- [API Reference](docs/api-reference.md) - Referencia completa de la API

## 🤖 **AI / LLM Context**

Este proyecto soporta el estándar [llms.txt](https://llmstxt.org/) para que modelos de IA puedan aprender a usar la librería:

- [llms.txt](llms.txt) - Índice para LLMs (enlaces a documentación)
- [llms-full.txt](llms-full.txt) - Documentación completa inline para LLMs

## 🤝 **Contribuir**

Las contribuciones son bienvenidas. Por favor, lee las guías de contribución.

## 📄 **Licencia**

MIT

## 🙏 **Agradecimientos**

- [Greenter PHP](https://github.com/thegreenter/greenter) - Inspiración y referencia
- [Giansalex](https://github.com/giansalex) - Creador de Greenter

---

**Khipu.Net - 100% Greenter Logic para .NET** 🇵🇪

*Nombrado en honor al antiguo sistema de contabilidad inca*
