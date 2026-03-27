namespace Khipu.Data.Documents;

using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Data.Common;

/// <summary>
/// Clase base para comprobantes de venta - Paridad 100% con Greenter BaseSale
/// </summary>
public abstract class BaseSale
{
    public Company Company { get; set; } = new();
    public Client Client { get; set; } = new();
    public VoucherType TipoDoc { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Correlativo { get; set; }
    public DateTime FechaEmision { get; set; }
    public Currency Moneda { get; set; } = Currency.Pen;
    public List<SaleDetail> Details { get; set; } = new();
    public List<Legend>? Leyendas { get; set; }

    // --- Operaciones por tipo ---
    public decimal MtoOperGravadas { get; set; }
    public decimal MtoOperExoneradas { get; set; }
    public decimal MtoOperInafectas { get; set; }
    public decimal MtoOperExportacion { get; set; }

    /// <summary>
    /// Operaciones gratuitas (Greenter: mtoOperGratuitas)
    /// </summary>
    public decimal MtoOperGratuitas { get; set; }

    // --- Impuestos ---
    public decimal MtoIGV { get; set; }

    /// <summary>
    /// IGV de operaciones gratuitas (Greenter: mtoIGVGratuitas)
    /// </summary>
    public decimal MtoIGVGratuitas { get; set; }

    /// <summary>
    /// Base imponible IVAP (Greenter: mtoBaseIvap)
    /// </summary>
    public decimal MtoBaseIvap { get; set; }

    /// <summary>
    /// Monto IVAP (Greenter: mtoIvap)
    /// </summary>
    public decimal MtoIvap { get; set; }

    /// <summary>
    /// Base imponible ISC (Greenter: mtoBaseIsc)
    /// </summary>
    public decimal MtoBaseIsc { get; set; }

    public decimal MtoISC { get; set; }

    /// <summary>
    /// Base imponible otros tributos (Greenter: mtoBaseOth)
    /// </summary>
    public decimal MtoBaseOth { get; set; }

    public decimal MtoOtrosTributos { get; set; }

    /// <summary>
    /// Total ICBPER (Greenter: icbper)
    /// </summary>
    public decimal Icbper { get; set; }

    public decimal TotalImpuestos { get; set; }
    public decimal Redondeo { get; set; }
    public decimal MtoImpVenta { get; set; }

    /// <summary>
    /// Suma de otros cargos (Greenter: sumOtrosCargos)
    /// </summary>
    public decimal? SumOtrosCargos { get; set; }

    // --- Documentos relacionados ---
    /// <summary>
    /// Guías de remisión relacionadas (Greenter: guias)
    /// </summary>
    public List<Document>? Guias { get; set; }

    /// <summary>
    /// Documentos relacionados (Greenter: relDocs)
    /// </summary>
    public List<Document>? RelDocs { get; set; }

    /// <summary>
    /// Orden de compra (Greenter: compra)
    /// </summary>
    public string? Compra { get; set; }

    // --- Forma de pago ---
    /// <summary>
    /// Forma de pago (Greenter: formaPago)
    /// </summary>
    public PaymentTerms? FormaPago { get; set; }

    /// <summary>
    /// Cuotas de pago a crédito (Greenter: cuotas)
    /// </summary>
    public List<Cuota>? Cuotas { get; set; }
}
