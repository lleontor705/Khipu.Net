# Getting Started

## Installation

```bash
dotnet add package Khipu.Net
```

## Prerequisites

- .NET 10 SDK
- A SUNAT digital certificate (.pfx) for signing documents
- SUNAT credentials (SOL user/password)

## Project setup

```csharp
using Khipu.Core.Factory;
using Khipu.Core.Builder;
using Khipu.Core.Services;
using Khipu.Core.Security;
using Khipu.Core.Algorithms;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
```

## Company configuration

All documents require a company (issuer):

```csharp
var company = new Company
{
    Ruc = "20123456789",
    RazonSocial = "MI EMPRESA SAC",
    NombreComercial = "Mi Empresa",
    Address = new Address
    {
        Ubigeo = "150101",
        Departamento = "LIMA",
        Provincia = "LIMA",
        Distrito = "LIMA",
        Direccion = "AV. REPUBLICA DE CHILE 295"
    }
};
```

## SUNAT environments

| Environment | Use |
|-------------|-----|
| `SunatEndpoints.Beta` | Testing (SUNAT sandbox) |
| `SunatEndpoints.Production` | Production |
