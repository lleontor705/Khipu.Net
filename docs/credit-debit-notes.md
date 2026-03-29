# Credit and Debit Notes

## Credit notes

Credit notes reference the original document being corrected.

```csharp
var factory = new InvoiceFactory(company);

var creditNote = factory.CreateCreditNote(
    client: new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SAC"
    },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now,
    docAfectado: "F001-00000001",   // Original document
    codMotivo: "01",                 // Catalogo 09: anulacion
    desMotivo: "Anulacion de la operacion"
);

creditNote.Details.Add(factory.CreateDetail(
    codigo: "PROD001",
    descripcion: "Laptop HP ProBook 450",
    unidad: "NIU",
    cantidad: 2,
    valorUnitario: 1500,
    tipoAfectacion: TaxType.Gravado
));
```

### Reason codes (Catalogo 09 - Credit Notes)

| Code | Description |
|------|-------------|
| 01 | Anulacion de la operacion |
| 02 | Anulacion por error en el RUC |
| 03 | Correccion por error en la descripcion |
| 04 | Descuento global |
| 05 | Descuento por item |
| 06 | Devolucion total |
| 07 | Devolucion parcial |
| 08 | Bonificacion |
| 09 | Disminucion en el valor |

## Debit notes

```csharp
var debitNote = factory.CreateDebitNote(
    client: new Client
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20987654321",
        RznSocial = "CLIENTE SAC"
    },
    serie: "F001",
    correlativo: 1,
    fechaEmision: DateTime.Now,
    docAfectado: "F001-00000001",
    codMotivo: "01",                 // Catalogo 10: intereses por mora
    desMotivo: "Intereses por mora"
);

debitNote.Details.Add(factory.CreateDetail(
    codigo: "INT001",
    descripcion: "Intereses por mora",
    unidad: "ZZ",
    cantidad: 1,
    valorUnitario: 50,
    tipoAfectacion: TaxType.Gravado
));
```

### Reason codes (Catalogo 10 - Debit Notes)

| Code | Description |
|------|-------------|
| 01 | Intereses por mora |
| 02 | Aumento en el valor |
| 03 | Penalidades |

## Sending to SUNAT

Credit and debit notes are sent individually, like invoices:

```csharp
var response = await service.SendCreditNoteAsync(creditNote);
```
