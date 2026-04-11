namespace Khipu.Data.Documents;

using Khipu.Data.Enums;
using Khipu.Data.Common;

/// <summary>
/// Detalle del comprobante de venta - Paridad 100% con Greenter SaleDetail
/// </summary>
public class SaleDetail
{
    public int Orden { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Unidad { get; set; } = "NIU"; // Catálogo 03
    public decimal Cantidad { get; set; }
    public decimal MtoValorUnitario { get; set; }
    public decimal MtoValorVenta { get; set; }

    /// <summary>
    /// Precio de venta unitario con impuestos (Greenter: mtoPrecioUnitario)
    /// </summary>
    public decimal? MtoPrecioUnitario { get; set; }

    /// <summary>
    /// Precio de venta total con impuestos (mantiene compatibilidad)
    /// </summary>
    public decimal PrecioVenta { get; set; }

    public decimal? Descuento { get; set; }

    // --- IGV ---
    /// <summary>
    /// Tipo de afectación del IGV (Catálogo 07)
    /// </summary>
    public TaxType TipoAfectacionIgv { get; set; } = TaxType.Gravado;

    /// <summary>
    /// Monto base para IGV
    /// </summary>
    public decimal? MtoBaseIgv { get; set; }

    /// <summary>
    /// Porcentaje IGV (ej: 18.00)
    /// </summary>
    public decimal? PorcentajeIgv { get; set; }

    /// <summary>
    /// Tasa de IGV como factor (0.18) - mantiene compatibilidad
    /// </summary>
    public decimal TasaIgv { get; set; } = 0.18m;

    /// <summary>
    /// Monto de IGV calculado para esta línea
    /// </summary>
    public decimal? Igv { get; set; }

    // --- ISC ---
    /// <summary>
    /// Monto base para ISC
    /// </summary>
    public decimal? MtoBaseIsc { get; set; }

    /// <summary>
    /// Porcentaje ISC
    /// </summary>
    public decimal? PorcentajeIsc { get; set; }

    /// <summary>
    /// Tasa de ISC como factor
    /// </summary>
    public decimal? TasaIsc { get; set; }

    /// <summary>
    /// Monto de ISC
    /// </summary>
    public decimal? MtoIsc { get; set; }

    /// <summary>
    /// Tipo de sistema ISC (Catálogo 08)
    /// </summary>
    public string? TipSisIsc { get; set; }

    // --- Otros Tributos ---
    /// <summary>
    /// Monto base para otros tributos
    /// </summary>
    public decimal? MtoBaseOth { get; set; }

    /// <summary>
    /// Porcentaje de otros tributos
    /// </summary>
    public decimal? PorcentajeOth { get; set; }

    /// <summary>
    /// Monto de otros tributos
    /// </summary>
    public decimal? OtroTributo { get; set; }

    // --- ICBPER ---
    /// <summary>
    /// Monto ICBPER (impuesto bolsas plásticas)
    /// </summary>
    public decimal? Icbper { get; set; }

    /// <summary>
    /// Factor ICBPER (default 0.30 por Greenter)
    /// </summary>
    public decimal FactorIcbper { get; set; } = 0.30m;

    // --- Totales ---
    /// <summary>
    /// Total de impuestos de la línea
    /// </summary>
    public decimal? TotalImpuestos { get; set; }

    // --- Gratuita ---
    /// <summary>
    /// Valor referencial unitario en operaciones gratuitas
    /// </summary>
    public decimal? MtoValorGratuito { get; set; }

    // --- Códigos producto ---
    /// <summary>
    /// Código de tributo (Catálogo 05)
    /// </summary>
    public string? CodTributo { get; set; }

    /// <summary>
    /// Código de producto SUNAT
    /// </summary>
    public string? CodProdSunat { get; set; }

    /// <summary>
    /// Código de producto GS1
    /// </summary>
    public string? CodProdGS1 { get; set; }

    // --- Cargos y descuentos de línea ---
    /// <summary>
    /// Cargos de línea (Catálogo 53)
    /// </summary>
    public List<Charge>? Cargos { get; set; }

    /// <summary>
    /// Descuentos de línea (Catálogo 53)
    /// </summary>
    public List<Charge>? Descuentos { get; set; }

    // --- Leyendas ---
    public List<Legend>? Leyendas { get; set; }

    /// <summary>Atributos adicionales del ítem (Greenter: atributos)</summary>
    public List<DetailAttribute>? Atributos { get; set; }
}
