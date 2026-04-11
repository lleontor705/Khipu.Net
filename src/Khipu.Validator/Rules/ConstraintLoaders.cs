namespace Khipu.Validator.Rules;

using System.Text.RegularExpressions;
using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Validator.Contracts;

/// <summary>
/// Constraint loaders - Paridad 1:1 con los 33 loaders de Greenter/Symfony Validator.
/// Cada método replica exactamente las constraints del loader PHP correspondiente.
/// </summary>
public static class ConstraintLoaders
{
    // ===== CompanyLoader =====
    public static List<ValidationError> LoadCompany(Company? c, string p = "company")
    {
        var e = new List<ValidationError>();
        if (c == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, c.Ruc, $"{p}.ruc");
        if (!string.IsNullOrEmpty(c.Ruc) && !Regex.IsMatch(c.Ruc, @"^\d{11}$"))
            e.Add(Err("Regex", $"{p}.ruc", "RUC debe tener 11 dígitos"));
        NotBlank(e, c.RazonSocial, $"{p}.razonSocial");
        MaxLen(e, c.RazonSocial, 100, $"{p}.razonSocial");
        MaxLen(e, c.NombreComercial, 100, $"{p}.nombreComercial");
        if (c.Address != null) e.AddRange(LoadAddress(c.Address, $"{p}.address"));
        return e;
    }

    // ===== ClientLoader =====
    public static List<ValidationError> LoadClient(Client? c, string p = "client")
    {
        var e = new List<ValidationError>();
        if (c == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, ((int)c.TipoDoc).ToString(), $"{p}.tipoDoc");
        NotBlank(e, c.NumDoc, $"{p}.numDoc");
        MaxLen(e, c.NumDoc, 15, $"{p}.numDoc");
        NotBlank(e, c.RznSocial, $"{p}.rznSocial");
        MaxLen(e, c.RznSocial, 100, $"{p}.rznSocial");
        if (c.Address != null) e.AddRange(LoadAddress(c.Address, $"{p}.address"));
        return e;
    }

    // ===== AddressLoader =====
    public static List<ValidationError> LoadAddress(Address? a, string p = "address")
    {
        var e = new List<ValidationError>();
        if (a == null) return e;
        NotBlank(e, a.Ubigeo, $"{p}.ubigeo");
        MaxLen(e, a.Ubigeo, 6, $"{p}.ubigeo");
        MaxLen(e, a.Departamento, 30, $"{p}.departamento");
        MaxLen(e, a.Provincia, 30, $"{p}.provincia");
        MaxLen(e, a.Distrito, 100, $"{p}.distrito");
        MaxLen(e, a.Urbanizacion, 25, $"{p}.urbanizacion");
        MaxLen(e, a.Direccion, 100, $"{p}.direccion");
        NotBlank(e, a.CodigoLocal, $"{p}.codLocal");
        return e;
    }

    // ===== InvoiceLoader =====
    public static List<ValidationError> LoadInvoice(Invoice? inv, string p = "invoice")
    {
        var e = new List<ValidationError>();
        if (inv == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, inv.TipoOperacion, $"{p}.tipoDoc");
        NotBlank(e, inv.Serie, $"{p}.serie");
        NotBlank(e, inv.Correlativo.ToString(), $"{p}.correlativo");
        if (!Regex.IsMatch(inv.Correlativo.ToString(), @"^\d{1,8}$"))
            e.Add(Err("Regex:G001", $"{p}.correlativo", "Correlativo debe ser numérico de 1-8 dígitos"));
        NotNull(e, inv.FechaEmision, $"{p}.fechaEmision");
        NotNull(e, inv.MtoOperGravadas, $"{p}.mtoOperGravadas");
        NotNull(e, inv.MtoOperInafectas, $"{p}.mtoOperInafectas");
        NotNull(e, inv.MtoOperExoneradas, $"{p}.mtoOperExoneradas");
        NotNull(e, inv.MtoImpVenta, $"{p}.mtoImpVenta");
        e.AddRange(LoadCompany(inv.Company, $"{p}.company"));
        e.AddRange(LoadClient(inv.Client, $"{p}.client"));
        foreach (var (d, i) in inv.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadSaleDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    // ===== NoteLoader (CreditNote/DebitNote) =====
    public static List<ValidationError> LoadNote(BaseSale note, string tipDoc, string numDocAfectado,
        string codMotivo, string desMotivo, string tipDocAfectado, string p = "note")
    {
        var e = new List<ValidationError>();
        NotBlank(e, tipDoc, $"{p}.tipoDoc");
        ExactLen(e, tipDoc, 2, $"{p}.tipoDoc");
        NotBlank(e, note.Serie, $"{p}.serie");
        MaxLen(e, note.Serie, 4, $"{p}.serie");
        NotBlank(e, note.Correlativo.ToString(), $"{p}.correlativo");
        MaxLen(e, note.Correlativo.ToString(), 8, $"{p}.correlativo");
        NotNull(e, note.FechaEmision, $"{p}.fechaEmision");
        NotNull(e, note.MtoOperGravadas, $"{p}.mtoOperGravadas");
        NotNull(e, note.MtoOperInafectas, $"{p}.mtoOperInafectas");
        NotNull(e, note.MtoOperExoneradas, $"{p}.mtoOperExoneradas");
        NotNull(e, note.MtoImpVenta, $"{p}.mtoImpVenta");
        NotBlank(e, codMotivo, $"{p}.codMotivo");
        ExactLen(e, codMotivo, 2, $"{p}.codMotivo");
        NotBlank(e, desMotivo, $"{p}.desMotivo");
        MaxLen(e, desMotivo, 250, $"{p}.desMotivo");
        NotBlank(e, tipDocAfectado, $"{p}.tipDocAfectado");
        ExactLen(e, tipDocAfectado, 2, $"{p}.tipDocAfectado");
        NotBlank(e, numDocAfectado, $"{p}.numDocAfectado");
        MaxLen(e, numDocAfectado, 13, $"{p}.numDocAfectado");
        e.AddRange(LoadCompany(note.Company, $"{p}.company"));
        e.AddRange(LoadClient(note.Client, $"{p}.client"));
        foreach (var (d, i) in note.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadSaleDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    public static List<ValidationError> LoadCreditNote(CreditNote n, string p = "creditNote")
        => LoadNote(n, "07", n.NumDocAfectado, n.CodMotivo, n.DesMotivo, n.TipDocAfectado, p);

    public static List<ValidationError> LoadDebitNote(DebitNote n, string p = "debitNote")
        => LoadNote(n, "08", n.NumDocAfectado, n.CodMotivo, n.DesMotivo, n.TipDocAfectado, p);

    // ===== SaleDetailLoader =====
    public static List<ValidationError> LoadSaleDetail(SaleDetail? d, string p = "detail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotBlank(e, d.Unidad, $"{p}.unidad");
        NotBlank(e, d.Descripcion, $"{p}.descripcion");
        MaxLen(e, d.Descripcion, 250, $"{p}.descripcion");
        NotNull(e, d.Cantidad, $"{p}.cantidad");
        MaxLen(e, d.Codigo, 30, $"{p}.codProducto");
        MaxLen(e, d.CodProdSunat, 20, $"{p}.codProdSunat");
        NotNull(e, d.MtoValorUnitario, $"{p}.mtoValorUnitario");
        NotNull(e, d.MtoValorVenta, $"{p}.mtoValorVenta");
        return e;
    }

    // ===== DespatchLoader =====
    public static List<ValidationError> LoadDespatch(Despatch? d, string p = "despatch")
    {
        var e = new List<ValidationError>();
        if (d == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, d.Serie, $"{p}.serie");
        MaxLen(e, d.Serie, 4, $"{p}.serie");
        NotBlank(e, d.Correlativo.ToString(), $"{p}.correlativo");
        MaxLen(e, d.DesMotivoTraslado, 250, $"{p}.observacion");
        NotNull(e, d.FechaEmision, $"{p}.fechaEmision");
        e.AddRange(LoadClient(d.Destinatario, $"{p}.destinatario"));
        e.AddRange(LoadCompany(d.Company, $"{p}.company"));
        // Shipment constraints
        NotBlank(e, d.CodMotivoTraslado, $"{p}.envio.codTraslado");
        ExactLen(e, d.CodMotivoTraslado, 2, $"{p}.envio.codTraslado");
        if (d.PesoTotal == null || d.PesoTotal <= 0)
            e.Add(Err("NotBlank", $"{p}.envio.pesoTotal", "Peso total es obligatorio"));
        NotBlank(e, d.UndPesoTotal, $"{p}.envio.undPesoTotal");
        MaxLen(e, d.UndPesoTotal, 4, $"{p}.envio.undPesoTotal");
        if (d.PuntoLlegada != null) e.AddRange(LoadDirection(d.PuntoLlegada, $"{p}.envio.llegada"));
        if (d.PuntoPartida != null) e.AddRange(LoadDirection(d.PuntoPartida, $"{p}.envio.partida"));
        if (d.Transportista != null) e.AddRange(LoadTransportist(d.Transportista, $"{p}.envio.transportista"));
        if (d.Details.Count == 0) e.Add(Err("NotBlank", $"{p}.details", "Debe incluir detalles"));
        foreach (var (det, i) in d.Details.Select((det, i) => (det, i)))
            e.AddRange(LoadDespatchDetail(det, $"{p}.details[{i}]"));
        return e;
    }

    // ===== DespatchDetailLoader =====
    public static List<ValidationError> LoadDespatchDetail(DespatchDetail? d, string p = "despatchDetail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        MaxLen(e, d.Codigo, 16, $"{p}.codigo");
        NotBlank(e, d.Descripcion, $"{p}.descripcion");
        MaxLen(e, d.Descripcion, 250, $"{p}.descripcion");
        NotBlank(e, d.Unidad, $"{p}.unidad");
        NotNull(e, d.Cantidad, $"{p}.cantidad");
        MaxLen(e, d.CodProdSunat, 20, $"{p}.codProdSunat");
        return e;
    }

    // ===== DirectionLoader =====
    public static List<ValidationError> LoadDirection(Address? a, string p = "direction")
    {
        var e = new List<ValidationError>();
        if (a == null) return e;
        NotBlank(e, a.Ubigeo, $"{p}.ubigueo");
        MaxLen(e, a.Ubigeo, 8, $"{p}.ubigueo");
        NotBlank(e, a.Direccion, $"{p}.direccion");
        MaxLen(e, a.Direccion, 100, $"{p}.direccion");
        return e;
    }

    // ===== TransportistLoader =====
    public static List<ValidationError> LoadTransportist(Transportist? t, string p = "transportista")
    {
        var e = new List<ValidationError>();
        if (t == null) return e;
        NotBlank(e, t.TipoDoc, $"{p}.tipoDoc");
        NotBlank(e, t.NumDoc, $"{p}.numDoc");
        MaxLen(e, t.NumDoc, 15, $"{p}.numDoc");
        NotBlank(e, t.RznSocial, $"{p}.rznSocial");
        MaxLen(e, t.RznSocial, 100, $"{p}.rznSocial");
        MaxLen(e, t.Placa, 8, $"{p}.placa");
        return e;
    }

    // ===== PerceptionLoader =====
    public static List<ValidationError> LoadPerception(Perception? per, string p = "perception")
    {
        var e = new List<ValidationError>();
        if (per == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, per.Serie, $"{p}.serie");
        MaxLen(e, per.Serie, 4, $"{p}.serie");
        NotBlank(e, per.Correlativo.ToString(), $"{p}.correlativo");
        MaxLen(e, per.Correlativo.ToString(), 8, $"{p}.correlativo");
        NotNull(e, per.FechaEmision, $"{p}.fechaEmision");
        e.AddRange(LoadCompany(per.Company, $"{p}.company"));
        e.AddRange(LoadClient(per.Proveedor, $"{p}.proveedor"));
        NotNull(e, per.MtoPercepcion, $"{p}.impPercibido");
        NotNull(e, per.MtoTotalCobrar, $"{p}.impCobrado");
        if (per.Details.Count == 0) e.Add(Err("NotBlank", $"{p}.details", "Debe incluir detalles"));
        foreach (var (d, i) in per.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadPerceptionDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    // ===== PerceptionDetailLoader =====
    public static List<ValidationError> LoadPerceptionDetail(PerceptionDetail? d, string p = "perceptionDetail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotBlank(e, d.TipoDoc, $"{p}.tipoDoc");
        NotBlank(e, d.NumDoc, $"{p}.numDoc");
        NotNull(e, d.FechaEmision, $"{p}.fechaEmision");
        NotNull(e, d.ImpTotal, $"{p}.impTotal");
        NotBlank(e, d.CodMoneda, $"{p}.moneda");
        return e;
    }

    // ===== RetentionLoader =====
    public static List<ValidationError> LoadRetention(Retention? ret, string p = "retention")
    {
        var e = new List<ValidationError>();
        if (ret == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, ret.Serie, $"{p}.serie");
        MaxLen(e, ret.Serie, 4, $"{p}.serie");
        NotBlank(e, ret.Correlativo.ToString(), $"{p}.correlativo");
        MaxLen(e, ret.Correlativo.ToString(), 8, $"{p}.correlativo");
        NotNull(e, ret.FechaEmision, $"{p}.fechaEmision");
        e.AddRange(LoadCompany(ret.Company, $"{p}.company"));
        e.AddRange(LoadClient(ret.Proveedor, $"{p}.proveedor"));
        NotNull(e, ret.MtoRetencion, $"{p}.impRetenido");
        NotNull(e, ret.MtoTotal, $"{p}.impPagado");
        if (ret.Details.Count == 0) e.Add(Err("NotBlank", $"{p}.details", "Debe incluir detalles"));
        foreach (var (d, i) in ret.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadRetentionDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    // ===== RetentionDetailLoader =====
    public static List<ValidationError> LoadRetentionDetail(RetentionDetail? d, string p = "retentionDetail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotBlank(e, d.TipoDoc, $"{p}.tipoDoc");
        NotBlank(e, d.NumDoc, $"{p}.numDoc");
        NotNull(e, d.FechaEmision, $"{p}.fechaEmision");
        NotNull(e, d.ImpTotal, $"{p}.impTotal");
        NotBlank(e, d.CodMoneda, $"{p}.moneda");
        return e;
    }

    // ===== SummaryLoader =====
    public static List<ValidationError> LoadSummary(Summary? s, string p = "summary")
    {
        var e = new List<ValidationError>();
        if (s == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, s.Correlativo, $"{p}.correlativo");
        MaxLen(e, s.Correlativo, 5, $"{p}.correlativo");
        NotNull(e, s.FechaGeneracion, $"{p}.fecGeneracion");
        NotNull(e, s.FechaEnvio, $"{p}.fecResumen");
        e.AddRange(LoadCompany(s.Company, $"{p}.company"));
        if (s.Details.Count == 0) e.Add(Err("NotBlank", $"{p}.details", "Debe incluir detalles"));
        foreach (var (d, i) in s.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadSummaryDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    // ===== SummaryDetailLoader =====
    public static List<ValidationError> LoadSummaryDetail(SummaryDetail? d, string p = "summaryDetail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotBlank(e, d.SerieNro, $"{p}.serieNro");
        NotBlank(e, d.ClienteTipoDoc, $"{p}.clienteTipoDoc");
        NotBlank(e, d.ClienteNroDoc, $"{p}.clienteNroDoc");
        NotNull(e, d.Total, $"{p}.total");
        return e;
    }

    // ===== VoidedLoader =====
    public static List<ValidationError> LoadVoided(Voided? v, string p = "voided")
    {
        var e = new List<ValidationError>();
        if (v == null) { e.Add(Err("NotNull", p)); return e; }
        NotBlank(e, v.Correlativo, $"{p}.correlativo");
        MaxLen(e, v.Correlativo, 5, $"{p}.correlativo");
        NotNull(e, v.FechaGeneracion, $"{p}.fecGeneracion");
        NotNull(e, v.FechaEnvio, $"{p}.fecComunicacion");
        e.AddRange(LoadCompany(v.Company, $"{p}.company"));
        if (v.Details.Count == 0) e.Add(Err("NotBlank", $"{p}.details", "Debe incluir detalles"));
        foreach (var (d, i) in v.Details.Select((d, i) => (d, i)))
            e.AddRange(LoadVoidedDetail(d, $"{p}.details[{i}]"));
        return e;
    }

    // ===== VoidedDetailLoader =====
    public static List<ValidationError> LoadVoidedDetail(VoidedDetail? d, string p = "voidedDetail")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotBlank(e, d.TipoDoc, $"{p}.tipoDoc");
        NotBlank(e, d.SerieNro, $"{p}.serieNro");
        NotBlank(e, d.MotivoBaja, $"{p}.desMotivoBaja");
        return e;
    }

    // ===== CuotaLoader =====
    public static List<ValidationError> LoadCuota(Cuota? c, string p = "cuota")
    {
        var e = new List<ValidationError>();
        if (c == null) return e;
        ExactLen(e, c.Moneda, 3, $"{p}.moneda");
        NotNull(e, c.Monto, $"{p}.monto");
        NotNull(e, c.FechaPago, $"{p}.fechaPago");
        return e;
    }

    // ===== DetractionLoader =====
    public static List<ValidationError> LoadDetraction(Detraction? d, string p = "detraccion")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        NotNull(e, d.Porcentaje, $"{p}.percent");
        NotNull(e, d.Monto, $"{p}.monto");
        return e;
    }

    // ===== DocumentLoader =====
    public static List<ValidationError> LoadDocument(Document? d, string p = "document")
    {
        var e = new List<ValidationError>();
        if (d == null) return e;
        ExactLen(e, d.TipoDoc, 2, $"{p}.tipoDoc");
        MaxLen(e, d.NroDoc, 30, $"{p}.nroDoc");
        return e;
    }

    // ===== LegendLoader =====
    public static List<ValidationError> LoadLegend(Legend? l, string p = "legend")
    {
        var e = new List<ValidationError>();
        if (l == null) return e;
        ExactLen(e, l.Code, 4, $"{p}.code");
        MaxLen(e, l.Value, 100, $"{p}.value");
        return e;
    }

    // ===== PrepaymentLoader =====
    public static List<ValidationError> LoadPrepayment(Prepayment? pp, string p = "prepayment")
    {
        var e = new List<ValidationError>();
        if (pp == null) return e;
        NotBlank(e, pp.TipoDoc, $"{p}.tipoDocRel");
        ExactLen(e, pp.TipoDoc, 2, $"{p}.tipoDocRel");
        NotBlank(e, pp.NroDoc, $"{p}.nroDocRel");
        MaxLen(e, pp.NroDoc, 30, $"{p}.nroDocRel");
        NotNull(e, pp.Total, $"{p}.total");
        return e;
    }

    // ===== PaymentLoader (Retention/Perception payments) =====
    public static List<ValidationError> LoadPayment(Payment? pay, string p = "payment")
    {
        var e = new List<ValidationError>();
        if (pay == null) return e;
        NotBlank(e, pay.FormaPago, $"{p}.moneda");
        NotNull(e, pay.Monto, $"{p}.importe");
        return e;
    }

    // ===== SalePerceptionLoader =====
    public static List<ValidationError> LoadSalePerception(SummaryPerception? sp, string p = "perception")
    {
        var e = new List<ValidationError>();
        if (sp == null) return e;
        NotBlank(e, sp.CodReg, $"{p}.codReg");
        NotNull(e, sp.MtoBase, $"{p}.mtoBase");
        NotNull(e, sp.Mto, $"{p}.mto");
        NotNull(e, sp.MtoTotal, $"{p}.mtoTotal");
        return e;
    }

    // ===== SummaryPerceptionLoader =====
    public static List<ValidationError> LoadSummaryPerception(SummaryPerception? sp, string p = "summaryPerception")
    {
        var e = new List<ValidationError>();
        if (sp == null) return e;
        NotBlank(e, sp.CodReg, $"{p}.codReg");
        NotNull(e, sp.Tasa, $"{p}.tasa");
        NotNull(e, sp.MtoBase, $"{p}.mtoBase");
        NotNull(e, sp.Mto, $"{p}.mto");
        NotNull(e, sp.MtoTotal, $"{p}.mtoTotal");
        return e;
    }

    // ===== FormaPagoContadoLoader =====
    public static List<ValidationError> LoadFormaPagoContado(PaymentTerms? pt, string p = "formaPago")
    {
        var e = new List<ValidationError>();
        if (pt == null) return e;
        NotBlank(e, pt.Tipo, $"{p}.tipo");
        return e;
    }

    // ===== FormaPagoCreditoLoader =====
    public static List<ValidationError> LoadFormaPagoCredito(PaymentTerms? pt, string p = "formaPago")
    {
        var e = new List<ValidationError>();
        if (pt == null) return e;
        NotBlank(e, pt.Tipo, $"{p}.tipo");
        ExactLen(e, pt.Moneda, 3, $"{p}.moneda");
        NotNull(e, pt.Monto, $"{p}.monto");
        return e;
    }

    // ===== ChargeLoader (v21) =====
    public static List<ValidationError> LoadCharge(Charge? c, string p = "charge")
    {
        var e = new List<ValidationError>();
        if (c == null) return e;
        if (c.Monto.HasValue && c.Monto < 0)
            e.Add(Err("Range", $"{p}.monto", "Monto no puede ser negativo"));
        return e;
    }

    // ===== Constraint helpers =====
    private static void NotBlank(List<ValidationError> e, string? v, string path)
    {
        if (string.IsNullOrWhiteSpace(v))
            e.Add(Err("NotBlank", path, $"{path} no debe estar vacío"));
    }

    private static void NotNull(List<ValidationError> e, object? v, string path)
    {
        if (v == null || (v is DateTime dt && dt == default))
            e.Add(Err("NotNull", path, $"{path} es obligatorio"));
    }

    private static void MaxLen(List<ValidationError> e, string? v, int max, string path)
    {
        if (v != null && v.Length > max)
            e.Add(Err("Length", path, $"{path} excede longitud máxima de {max}"));
    }

    private static void ExactLen(List<ValidationError> e, string? v, int len, string path)
    {
        if (!string.IsNullOrEmpty(v) && v.Length != len)
            e.Add(Err("Length", path, $"{path} debe tener exactamente {len} caracteres"));
    }

    private static ValidationError Err(string code, string path, string? msg = null)
        => new($"VAL-{code}", path, msg ?? $"{path} no es válido");
}
