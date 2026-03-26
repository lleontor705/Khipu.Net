# Khipu.Net 🪢

Librería .NET Core 10 para Facturación Electrónica SUNAT Perú

## Descripción

Los **khipus** eran sistemas de contabilidad utilizados por los incas, consistían en cuerdas con nudos para registrar información numérica y administrativa.

**Khipu.Net** es una implementación moderna en .NET Core 10 para facturación electrónica en Perú, inspirada en la popular librería [Greenter](https://github.com/thegreenter/greenter).

## Características

- 🔤 Generación de XML según estándar UBL 2.1
- 🔏 Firma digital con certificados X.509
- 📤 Envío a webservices de SUNAT
- 📥 Procesamiento de CDR (Comprobante de Recepción)
- ✅ Validación de comprobantes

## Arquitectura

| Proyecto | Descripción |
|----------|-------------|
| **Khipu.Core** | Núcleo y funcionalidad base |
| **Khipu.Data** | Modelos y entidades de datos |
| **Khipu.Xml** | Generación y parsing de XML |
| **Khipu.Ws** | Cliente SOAP para webservices SUNAT |
| **Khipu.Validator** | Validaciones de comprobantes |

## Estado

🚧 En desarrollo

## Instalación

`ash
dotnet add package Khipu.Net
`

## Uso rápido

`csharp
using Khipu.Net;

// Próximamente...
`

## Licencia

MIT

## Inspiración

- [Greenter PHP](https://github.com/thegreenter/greenter) por Giansalex

---

*Nombrado en honor al antiguo sistema de contabilidad inca* 🇵🇪
