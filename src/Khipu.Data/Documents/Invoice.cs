namespace Khipu.Data.Documents;

using Khipu.Data.Common;
using Khipu.Data.Entities;

/// <summary>
/// Factura Electrónica - Paridad 100% con Greenter Invoice
/// </summary>
public class Invoice : BaseSale
{
    public Invoice()
    {
        TipoDoc = Enums.VoucherType.Factura;
    }

    public string? TipoOperacion { get; set; }
    public DateTime? FecVencimiento { get; set; }

    // --- Descuentos ---
    /// <summary>
    /// Suma de descuento global (Greenter: sumDsctoGlobal)
    /// </summary>
    public decimal? SumDsctoGlobal { get; set; }

    /// <summary>
    /// Monto total de descuentos (Greenter: mtoDescuentos)
    /// </summary>
    public decimal? MtoDescuentos { get; set; }

    /// <summary>
    /// Suma otros descuentos (Greenter: sumOtrosDescuentos)
    /// </summary>
    public decimal? SumOtrosDescuentos { get; set; }

    /// <summary>
    /// Descuentos detallados (Greenter: descuentos - Charge[])
    /// </summary>
    public List<Charge>? Descuentos { get; set; }

    // --- Cargos ---
    /// <summary>
    /// Cargos detallados (Greenter: cargos - Charge[])
    /// </summary>
    public List<Charge>? Cargos { get; set; }

    /// <summary>
    /// Monto total de cargos (Greenter: mtoCargos)
    /// </summary>
    public decimal? MtoCargos { get; set; }

    // --- Anticipos ---
    public decimal? TotalAnticipos { get; set; }
    public List<Prepayment>? Anticipos { get; set; }

    // --- Detracción ---
    public Detraction? Detraccion { get; set; }

    // --- Percepción ---
    /// <summary>
    /// Percepción de venta (Greenter: perception - SalePerception)
    /// </summary>
    public SalePerception? Perception { get; set; }

    // --- Vendedor ---
    /// <summary>
    /// Vendedor diferente al emisor (Greenter: seller)
    /// </summary>
    public Client? Seller { get; set; }

    // --- Totales adicionales ---
    /// <summary>
    /// Valor de venta (Greenter: valorVenta)
    /// </summary>
    public decimal? ValorVenta { get; set; }

    /// <summary>
    /// Subtotal (Greenter: subTotal)
    /// </summary>
    public decimal? SubTotal { get; set; }

    // --- Observación ---
    /// <summary>
    /// Observación/Notas (Greenter: observacion)
    /// </summary>
    public string? Observacion { get; set; }

    // --- Dirección de entrega ---
    /// <summary>
    /// Dirección de entrega (Greenter: direccionEntrega)
    /// </summary>
    public Address? DireccionEntrega { get; set; }

    // --- Guía embebida ---
    /// <summary>
    /// Guía de remisión embebida (Greenter: guiaEmbebida)
    /// </summary>
    public EmbededDespatch? GuiaEmbebida { get; set; }
}

/// <summary>
/// Percepción de venta - Paridad con Greenter SalePerception
/// </summary>
public class SalePerception
{
    public string? CodReg { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal MtoBase { get; set; }
    public decimal Mto { get; set; }
    public decimal MtoTotal { get; set; }
}

/// <summary>
/// Guía de remisión embebida - Paridad con Greenter EmbededDespatch
/// </summary>
public class EmbededDespatch
{
    public Address? Llegada { get; set; }
    public Address? Partida { get; set; }
    public string? CodTraslado { get; set; }
    public string? DesTraslado { get; set; }
}
