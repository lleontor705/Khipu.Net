# Summaries and Voided Documents

## Receipt summaries (resumen diario)

Receipts (boletas) must be reported to SUNAT via a daily summary, not sent individually.

```csharp
var summary = new Summary
{
    Company = company,
    Correlativo = "001",
    FechaGeneracion = DateTime.Now,
    FechaEnvio = DateTime.Now,
    Moneda = "PEN",
    Details = new List<SummaryDetail>
    {
        new()
        {
            Orden = 1,
            TipoDoc = VoucherType.Boleta,
            SerieNro = "B001-00000001",
            ClienteTipoDoc = "1",         // DNI
            ClienteNroDoc = "12345678",
            FechaDoc = DateTime.Now,
            Estado = "1",                  // 1=Add, 2=Modify, 3=Void
            MtoOperGravadas = 100,
            MtoIGV = 18,
            MtoImpVenta = 118,
            Total = 118
        }
    }
};

var response = await service.SendSummaryAsync(summary);
```

### Summary states

| Code | Description |
|------|-------------|
| 1 | Add (informar) |
| 2 | Modify (modificar) |
| 3 | Void (anular) |

### Polling for CDR

Summaries are processed asynchronously. Use the ticket to check status:

```csharp
// Option 1: Manual polling
var response = await service.SendSummaryAsync(summary);
if (response.Success && response.Ticket != null)
{
    var status = await service.GetStatusAsync(response.Ticket);
    // StatusCode "98" means still processing
}

// Option 2: Automatic polling (recommended)
var cdr = await service.SendSummaryAndQueryCdrAsync(
    summary,
    tipoComprobante: "03",
    serie: "B001",
    correlativo: 1,
    maxStatusChecks: 3,
    pollInterval: TimeSpan.FromSeconds(5)
);
```

## Voided documents (comunicacion de bajas)

Use voided documents to cancel invoices, credit notes, or debit notes.

```csharp
var voided = new Voided
{
    Company = company,
    Correlativo = "001",
    FechaGeneracion = DateTime.Now,
    FechaEnvio = DateTime.Now,
    Details = new List<VoidedDetail>
    {
        new()
        {
            Orden = 1,
            TipoDoc = "01",                    // 01=Factura
            SerieNro = "F001-00000001",
            FechaDoc = DateTime.Now.AddDays(-1),
            MotivoBaja = "Error en la emision"
        }
    }
};

var response = await service.SendVoidedAsync(voided);
```

> Voided documents are also processed asynchronously. Use `GetStatusAsync` to check the result.
