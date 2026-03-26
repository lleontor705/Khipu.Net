# Greenter.Net

Librería .NET Core 10 para Facturación Electrónica SUNAT Perú

## Descripción

Implementación en .NET Core 10 de la popular librería [Greenter](https://github.com/thegreenter/greenter) para facturación electrónica en Perú.

## Características

- Generación de XML según estándar UBL 2.1
- Firma digital con certificados X.509
- Envío a webservices de SUNAT
- Procesamiento de CDR (Comprobante de Recepción)
- Validación de comprobantes

## Arquitectura

- **Greenter.Core**: Núcleo y funcionalidad base
- **Greenter.Data**: Modelos y entidades de datos
- **Greenter.Xml**: Generación y parsing de XML
- **Greenter.Ws**: Cliente SOAP para webservices SUNAT
- **Greenter.Validator**: Validaciones de comprobantes

## Estado

🚧 En desarrollo

## Licencia

MIT

## Basado en

- [Greenter PHP](https://github.com/thegreenter/greenter) por Giansalex
