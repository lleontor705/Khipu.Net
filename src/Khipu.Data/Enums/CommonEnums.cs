namespace Khipu.Data.Enums;

/// <summary>
/// Tipos de moneda (Catálogo 02)
/// </summary>
public enum Currency
{
    Pen = 1,
    Usd = 2,
    Eur = 3
}

/// <summary>
/// Tipos de afectación IGV (Catálogo 07)
/// Mapeo a tributo (Greenter TributoFunction):
///   10 → 1000 IGV/VAT
///   17 → 1016 IVAP/VAT
///   20 → 9997 EXO/VAT
///   30 → 9998 INA/FRE
///   40 → 9995 EXP/FRE
///   default → 9996 GRA/FRE (Gratuito)
/// </summary>
public enum TaxType
{
    Gravado = 10,
    Ivap = 17,
    Exonerado = 20,
    Inafecto = 30,
    Exportacion = 40,
    Gratuito = 21
}
