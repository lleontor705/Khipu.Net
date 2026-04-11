# Getting Started

## Requisitos

- .NET 10.0 SDK
- Certificado digital X.509 (.pfx) para firma
- Clave SOL de SUNAT (usuario secundario)
- Para GRE REST API: Client ID y Client Secret de SUNAT

## Instalacion

```bash
dotnet add package Khipu.Net
```

## Configuracion de empresa

```csharp
using Khipu.Data.Entities;

var company = new Company
{
    Ruc = "20123456789",
    RazonSocial = "MI EMPRESA SAC",
    NombreComercial = "MI EMPRESA",
    Address = new Address
    {
        Ubigeo = "150101",
        Departamento = "LIMA",
        Provincia = "LIMA",
        Distrito = "LIMA",
        Direccion = "AV. PRINCIPAL 123",
        CodigoLocal = "0000",
        CodigoPais = "PE"
    }
};
```

## Firma digital

```csharp
using Khipu.Core.Security;

var signer = XmlSigner.FromPfx("certificado.pfx", "password");
```

## Servicio SUNAT

### SOAP (legacy)

```csharp
using Khipu.Core.Services;
using Khipu.Ws.Constants;

var service = new SunatService("20123456789MODDATOS", "clavesol", SunatEndpoints.Beta, signer);
```

### GRE REST API (moderno)

```csharp
using Khipu.Ws.Services;

var client = new GreClient("client-id", "client-secret", "20123456789", "MODDATOS", "clavesol");
var service = new SunatService(client, signer);
```

## Primer envio

```csharp
using Khipu.Core.Builder;

var invoice = new InvoiceBuilder()
    .WithCompany(company)
    .WithClient(new Client { TipoDoc = DocumentType.Ruc, NumDoc = "20987654321", RznSocial = "CLIENTE" })
    .WithSerie("F001")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Today)
    .AddDetail(new SaleDetail
    {
        Codigo = "P001", Descripcion = "Producto", Unidad = "NIU", Cantidad = 1,
        MtoValorUnitario = 100, MtoValorVenta = 100, PrecioVenta = 118,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build();

var response = await service.SendInvoiceAsync(invoice);
```

## Endpoints

| Entorno | Constante | Uso |
|---------|-----------|-----|
| Beta SOAP | `SunatEndpoints.Beta` | Pruebas |
| Produccion SOAP | `SunatEndpoints.Production` | Real |
| GRE Auth | `SunatEndpoints.GreAuth` | OAuth2 token |
| GRE CPE | `SunatEndpoints.GreCpe` | Envio REST |
