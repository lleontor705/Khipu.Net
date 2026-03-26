# Khipu.Net 🪢

Librería .NET Core 10 para Facturación Electrónica SUNAT Perú

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-29%20passing-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

## Descripción

Los **khipus** eran sistemas de contabilidad utilizados por los incas, consistían en cuerdas con nudos para registrar información numérica y administrativa.

**Khipu.Net** es una implementación moderna en .NET Core 10 para facturación electrónica en Perú, inspirada en la popular librería [Greenter](https://github.com/thegreenter/greenter).

## Estado del Proyecto

🚧 **En desarrollo activo**

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| **Khipu.Data** | ✅ 100% | Modelos y entidades completos |
| **Khipu.Core** | ✅ 80% | Builders, validación, constantes |
| **Khipu.Xml** | 🔄 60% | Generación XML UBL 2.1 (Invoice, CreditNote) |
| **Khipu.Ws** | ⏳ 0% | Cliente SOAP SUNAT |
| **Khipu.Validator** | ⏳ 0% | Validaciones avanzadas |

## Características

- 🔤 Modelos completos para todos los comprobantes SUNAT
- 🏗️ Builder pattern con fluent API
- ✅ Validación de RUC/DNI
- 📊 Cálculo automático de impuestos (IGV, ISC)
- 📄 Generación XML UBL 2.1
- 🧪 29+ tests unitarios

## Documentos Soportados

| Documento | Código | Modelo | XML Builder |
|-----------|--------|--------|-------------|
| Factura | 01 | ✅ | ✅ |
| Boleta de Venta | 03 | ✅ | ⏳ |
| Nota de Crédito | 07 | ✅ | ✅ |
| Nota de Débito | 08 | ✅ | ⏳ |
| Guía de Remisión | 09 | ✅ | ⏳ |
| Comprobante de Retención | 20 | ✅ | ⏳ |
| Comprobante de Percepción | 40 | ✅ | ⏳ |
| Resumen de Boletas | RC | ✅ | ⏳ |
| Comunicación de Bajas | RA | ✅ | ⏳ |

## Instalación

`ash
dotnet add package Khipu.Net
`

## Uso Rápido

### Crear Factura

`csharp
using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

var invoice = new InvoiceBuilder()
    .WithCompany(new Company 
    { 
        Ruc = "20123456789", 
        RazonSocial = "MI EMPRESA SAC" 
    })
    .WithClient(new Client 
    { 
        TipoDoc = DocumentType.Ruc, 
        NumDoc = "20987654321", 
        RznSocial = "CLIENTE SRL" 
    })
    .WithSerie("F001")
    .WithCorrelativo(123)
    .WithFechaEmision(DateTime.Now)
    .AddDetail(new SaleDetail
    {
        Codigo = "PROD001",
        Descripcion = "Producto de prueba",
        Cantidad = 2,
        MtoValorUnitario = 100,
        MtoValorVenta = 200
    })
    .Build();

// Validar
if (new InvoiceBuilder().Validate())
{
    // Documento válido
}
`

### Generar XML UBL 2.1

`csharp
using Khipu.Xml.Builder;

var xmlBuilder = new InvoiceXmlBuilder();
var xml = xmlBuilder.Build(invoice);
var fileName = xmlBuilder.GetFileName(invoice);
// fileName: "20123456789-01-F001-00000123.xml"
`

## Arquitectura

`
Khipu.Net/
├── src/
│   ├── Khipu.Core/         # Lógica de negocio, builders
│   │   ├── Builder/        # InvoiceBuilder, ReceiptBuilder
│   │   ├── Constants/      # SunatConstants (IGV, ISC, etc.)
│   │   └── Validation/     # DocumentValidator (RUC/DNI)
│   ├── Khipu.Data/         # Modelos y entidades
│   │   ├── Documents/      # Invoice, Receipt, CreditNote, etc.
│   │   ├── Entities/       # Company, Client, Address
│   │   ├── Common/         # Legend, Detraction, Prepayment
│   │   └── Enums/          # VoucherType, Currency, TaxType
│   ├── Khipu.Xml/          # Generación XML UBL 2.1
│   │   └── Builder/        # InvoiceXmlBuilder, CreditNoteXmlBuilder
│   ├── Khipu.Ws/           # Cliente SOAP SUNAT (TODO)
│   └── Khipu.Validator/    # Validaciones avanzadas (TODO)
└── tests/
    └── Khipu.Tests/        # Tests unitarios (29 passing)
`

## Roadmap

- [x] Modelos de datos
- [x] Builder pattern
- [x] Validación RUC/DNI
- [x] Cálculo de impuestos
- [x] Generación XML UBL 2.1 (Invoice)
- [x] Generación XML UBL 2.1 (CreditNote)
- [ ] Generación XML para todos los documentos
- [ ] Firma digital X.509
- [ ] Cliente SOAP SUNAT
- [ ] CDR parsing
- [ ] PDF generation

## Tests

`ash
dotnet test
# 29 tests passing
`

## Contribuir

Las contribuciones son bienvenidas. Por favor, lee las guías de contribución.

## Licencia

MIT

## Inspiración

- [Greenter PHP](https://github.com/thegreenter/greenter) por Giansalex

---

*Nombrado en honor al antiguo sistema de contabilidad inca* 🇵🇪
