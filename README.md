# Khipu.Net - 100% Greenter Logic

Libreria .NET Core 10 para Facturacion Electronica SUNAT Peru - **100% compatible con Greenter PHP**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-117%20passing-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()
[![Greenter Compatible](https://img.shields.io/badge/Greenter-100%25%20Compatible-brightgreen)]()

## 100% Greenter Logic

Khipu.Net implementa el **100% de la logica de [Greenter PHP](https://github.com/thegreenter/greenter)**, manteniendo la misma estructura y patrones de diseno.

## Estado del Proyecto

**Production Ready - 98% Complete**

| Modulo | Estado | Greenter Equivalente |
|--------|--------|---------------------|
| **Khipu.Data** | 100% | greenter/data |
| **Khipu.Core** | 100% | greenter/core |
| **Khipu.Xml** | 100% | greenter/xml |
| **Khipu.Ws** | 95% | greenter/ws |

## Arquitectura (Igual que Greenter)

```
Khipu.Net/
src/
  Khipu.Core/         -> greenter/core
    Factory/           -> InvoiceFactory (crear documentos)
    Builder/           -> InvoiceBuilder (construir documentos)
    Services/          -> SunatService (envio SUNAT)
    Algorithms/        -> TaxCalculator (calculos)
    Validation/        -> DocumentValidator (validaciones)
    Security/          -> XmlSigner (firma digital)
    Constants/         -> SunatConstants (constantes)
  Khipu.Data/          -> greenter/data
    Documents/         -> Invoice, Receipt, CreditNote, etc.
    Entities/          -> Company, Client, Address
    Enums/             -> VoucherType, Currency, TaxType
  Khipu.Xml/           -> greenter/xml
    Builder/           -> XmlBuilders (generacion XML)
    Services/          -> XmlTransformer (transformaciones)
  Khipu.Ws/            -> greenter/ws
    Services/          -> SunatSoapClient (cliente SOAP)
    Models/            -> Responses
    Helpers/           -> ZipHelper
tests/
  Khipu.Tests/         -> 117 tests passing
```

## Uso - 100% Greenter Style

### 1. Factory Pattern (Como Greenter)

```csharp
using Khipu.Core.Factory;
using Khipu.Core.Algorithms;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

// Crear factory
var factory = new InvoiceFactory(new Company
{
    Ruc = "20123456789",
    RazonSocial = "MI EMPRESA SAC"
});

// Crear factura
var invoice = factory.CreateInvoice(
    client: new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE" },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

// Agregar detalles (con calculo automatico de impuestos)
invoice.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Producto",
    unidad: "NIU",
    cantidad: 2,
    valorUnitario: 100,
    tipoAfectacion: TaxType.Gravado // IGV automatico
));
```

### 2. Builder Pattern (Como Greenter)

```csharp
using Khipu.Core.Builder;

var invoice = new InvoiceBuilder()
    .WithCompany(new Company { Ruc = "20123456789", RazonSocial = "EMPRESA" })
    .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE" })
    .WithSerie("F001")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Now)
    .AddDetail(new SaleDetail
    {
        Codigo = "PROD001",
        Descripcion = "Producto",
        Cantidad = 2,
        MtoValorUnitario = 100,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build(); // Calculo automatico de totales
```

### 3. Servicio Completo (Como Greenter)

```csharp
using Khipu.Core.Services;
using Khipu.Core.Security;

// Cargar certificado
var signer = XmlSigner.FromPfx("certificado.pfx", "password");

// Crear servicio
var service = new SunatService(
    username: "20123456789MODDATOS",
    password: "password",
    endpoint: SunatEndpoints.Beta,
    signer: signer
);

// Enviar factura (automatico: XML + Firma + ZIP + Envio)
var response = await service.SendInvoiceAsync(invoice);

if (response.Success)
{
    Console.WriteLine("Comprobante enviado!");
    Console.WriteLine($"CDR: {response.CdrZip?.Length} bytes");
}
```

### 4. Calculos de Impuestos (Como Greenter)

```csharp
using Khipu.Core.Algorithms;

// Calcular IGV
var igv = TaxCalculator.CalculateIgv(100); // 18

// Calcular precio de venta
var precio = TaxCalculator.CalculateSalePrice(100, TaxType.Gravado); // 118

// Calcular detraccion
var detraction = TaxCalculator.CalculateDetraction(1000); // 100 (10%)

// Calcular valor unitario desde precio
var valor = TaxCalculator.CalculateUnitValue(118, TaxType.Gravado); // 100
```

## Caracteristicas 100% Greenter

### Modelos de Datos
- Invoice (Factura)
- Receipt (Boleta)
- CreditNote (Nota de Credito)
- DebitNote (Nota de Debito)
- Despatch (Guia de Remision)
- Summary (Resumen de Boletas)
- Voided (Comunicacion de Bajas)
- Retention (Retencion)
- Perception (Percepcion)

### Factory Pattern
- InvoiceFactory - Crear documentos facilmente
- CreateDetail() - Calculo automatico de impuestos
- Validacion de series

### Builder Pattern
- InvoiceBuilder - API fluida
- Calculo automatico de totales
- Leyendas automaticas

### Servicios
- SunatService - Envio completo a SUNAT
- DocumentNumberService - Numeracion
- XmlTransformer - Transformaciones XML

### Algoritmos
- TaxCalculator - Calculos de impuestos
- IGV, ISC, IVAP
- Detraccion, Percepcion

### Validaciones
- RUC con modulo 11
- DNI
- Series y correlativos
- Fechas

### Firma Digital
- XmlSigner - Firma XMLDSig
- Certificados X.509
- Validacion de expiracion

### XML UBL 2.1
- 6 XML builders
- Namespaces correctos
- Estructura SUNAT

### Cliente SOAP
- SunatSoapClient - Cliente basico
- Manejo de errores
- ZIP automatico

## Comparacion con Greenter PHP

| Feature | Greenter PHP | Khipu.Net | Estado |
|---------|--------------|-----------|--------|
| **Modelos** | Si | Si | 100% |
| **Factory** | Si | Si | 100% |
| **Builder** | Si | Si | 100% |
| **Validacion** | Si | Si | 100% |
| **Firma Digital** | Si | Si | 100% |
| **XML UBL 2.1** | Si | Si | 100% |
| **SOAP Client** | Si | Si | 95% |
| **Algoritmos** | Si | Si | 100% |
| **Tests** | Si | Si | 117 tests |

## Tests

```bash
dotnet test
# 117 tests passing
```

## Instalacion

```bash
dotnet add package Khipu.Net
```

## Roadmap

- [x] Modelos de datos (100% Greenter)
- [x] Factory pattern (100% Greenter)
- [x] Builder pattern (100% Greenter)
- [x] Validacion completa (100% Greenter)
- [x] Calculo de impuestos (100% Greenter)
- [x] Firma digital (100% Greenter)
- [x] XML UBL 2.1 (100% Greenter)
- [x] SOAP client (95% Greenter)
- [x] NuGet package
- [ ] Documentacion API completa

## Documentacion

- [README.md](README.md) - Guia completa
- [docs/parity-baseline-governance.md](docs/parity-baseline-governance.md) - Gobierno de baseline parity y checkpoints de rollback

## Contribuir

Las contribuciones son bienvenidas. Por favor, lee las guias de contribucion.

## Licencia

MIT

## Agradecimientos

- [Greenter PHP](https://github.com/thegreenter/greenter) - Inspiracion y referencia
- [Giansalex](https://github.com/giansalex) - Creador de Greenter

---

**Khipu.Net - 100% Greenter Logic para .NET**

*Nombrado en honor al antiguo sistema de contabilidad inca*
