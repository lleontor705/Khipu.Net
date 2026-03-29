# Invoices and Receipts

## Creating invoices

### Using Factory (recommended)

The Factory pattern creates documents with sensible defaults and automatic tax calculation on line items.

```csharp
var factory = new InvoiceFactory(company);

// Create invoice (factura)
var invoice = factory.CreateInvoice(
    client: new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SAC"
    },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

// Add line items - IGV is calculated automatically
invoice.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Laptop HP ProBook 450",
    unidad: "NIU",       // Catalogo 03: NIU = unidad, ZZ = servicio
    cantidad: 2,
    valorUnitario: 1500,  // Value without tax
    tipoAfectacion: TaxType.Gravado
));

invoice.Details.Add(factory.CreateDetail(
    codigo: "SERV001",
    descripcion: "Servicio de instalacion",
    unidad: "ZZ",
    cantidad: 1,
    valorUnitario: 200,
    tipoAfectacion: TaxType.Gravado
));
```

### Using Builder

The Builder pattern provides a fluent API and calculates totals automatically on `Build()`.

```csharp
var invoice = new InvoiceBuilder()
    .WithCompany(company)
    .WithClient(new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SAC"
    })
    .WithSerie("F001")
    .WithCorrelativo(1)
    .WithFechaEmision(DateTime.Now)
    .AddDetail(new SaleDetail
    {
        Codigo = "PROD001",
        Descripcion = "Laptop HP ProBook 450",
        Unidad = "NIU",
        Cantidad = 2,
        MtoValorUnitario = 1500,
        TipoAfectacionIgv = TaxType.Gravado
    })
    .Build();
```

### Validation before building

```csharp
var builder = new InvoiceBuilder()
    .WithCompany(company)
    .WithSerie("F001");

if (!builder.Validate())
{
    foreach (var error in builder.GetErrors())
        Console.WriteLine(error);
    return;
}

var invoice = builder.Build();
```

## Creating receipts (boletas)

Receipts use the same factory but with `CreateReceipt`. The client typically uses DNI.

```csharp
var receipt = factory.CreateReceipt(
    client: new Client
    {
        TipoDoc = DocumentType.Dni,
        NumDoc = "12345678",
        RznSocial = "JUAN PEREZ"
    },
    serie: "B001",
    correlativo: 1,
    fechaEmision: DateTime.Now
);

receipt.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Producto",
    unidad: "NIU",
    cantidad: 1,
    valorUnitario: 50,
    tipoAfectacion: TaxType.Gravado
));
```

> Receipts must be sent via `Summary` (resumen diario), not individually.
> See [Summaries](summaries.md).

## Tax types

| TaxType | Code | Description | Tax rate |
|---------|------|-------------|----------|
| `Gravado` | 10 | Taxable (IGV applies) | 18% |
| `Exonerado` | 20 | Exempted | 0% |
| `Inafecto` | 30 | Non-taxable | 0% |
| `Exportacion` | 40 | Export | 0% |
| `Gratuito` | 21 | Free/gratuitous | 0% (reference IGV) |
| `Ivap` | 17 | IVAP (rice) | 4% |

## Series format

| Document type | Series prefix | Example |
|---------------|---------------|---------|
| Factura | F | F001 |
| Boleta | B | B001 |
| Nota de credito (factura) | F | F001 |
| Nota de credito (boleta) | B | B001 |
| Guia de remision | T or V | T001 |

## Invoice with payment terms

```csharp
// Cash payment
invoice.FormaPago = new PaymentTerms
{
    Tipo = "Contado",
    Moneda = "PEN",
    Monto = invoice.MtoImpVenta
};

// Credit payment with installments
invoice.FormaPago = new PaymentTerms
{
    Tipo = "Credito",
    Moneda = "PEN",
    Monto = invoice.MtoImpVenta
};

invoice.Cuotas = new List<Cuota>
{
    new() { Moneda = "PEN", Monto = 1000, FechaPago = DateTime.Now.AddDays(30) },
    new() { Moneda = "PEN", Monto = 1000, FechaPago = DateTime.Now.AddDays(60) }
};
```

## Invoice with detraction

```csharp
invoice.Detraccion = new Detraction
{
    CodBienDetraccion = "014",     // Catalogo 54
    Porcentaje = 0.12m,
    Mount = 240,
    CtaBanco = "00-123-456789",
    CodMedioPago = "001"           // Catalogo 59
};
```

## Invoice with perception

```csharp
invoice.Perception = new SalePerception
{
    CodReg = "01",          // Catalogo 22
    Porcentaje = 0.02m,
    MtoBase = 2000,
    Mto = 40,
    MtoTotal = 2040
};
```
