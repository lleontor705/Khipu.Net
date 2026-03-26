namespace Khipu.Core.Validation;

using Khipu.Core.Constants;

/// <summary>
/// Validador de documentos de identidad (basado en Greenter)
/// </summary>
public static class DocumentValidator
{
    /// <summary>
    /// Valida RUC peruano (11 dígitos) con algoritmo completo
    /// </summary>
    public static bool ValidateRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
            return false;
        
        if (ruc.Length != SunatConstants.RucLength)
            return false;
        
        if (!ruc.All(char.IsDigit))
            return false;
        
        // Validar prefijo (10, 15, 17, 20)
        var prefix = ruc[..2];
        var validPrefixes = new[] { "10", "15", "17", "20" };
        if (!validPrefixes.Contains(prefix))
            return false;
        
        // Algoritmo de verificación módulo 11
        var weights = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        
        for (int i = 0; i < 10; i++)
        {
            sum += (ruc[i] - '0') * weights[i];
        }
        
        var checkDigit = 11 - (sum % 11);
        
        // Ajustes según módulo 11
        if (checkDigit == 10)
            checkDigit = 0;
        else if (checkDigit == 11)
            checkDigit = 1;
        
        return (ruc[10] - '0') == checkDigit;
    }
    
    /// <summary>
    /// Valida DNI peruano (8 dígitos)
    /// </summary>
    public static bool ValidateDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            return false;
        
        if (dni.Length != SunatConstants.DniLength)
            return false;
        
        return dni.All(char.IsDigit);
    }
    
    /// <summary>
    /// Valida documento según tipo
    /// </summary>
    public static bool ValidateDocument(string tipoDoc, string numDoc)
    {
        if (string.IsNullOrWhiteSpace(numDoc))
            return false;
            
        return tipoDoc switch
        {
            "6" => ValidateRuc(numDoc),
            "1" => ValidateDni(numDoc),
            "4" => numDoc.Length <= 12, // Carnet de extranjería
            "7" => numDoc.Length <= 12, // Pasaporte
            "A" => numDoc.Length <= 15, // Cédula diplomática
            "0" => true, // Sin RUC (destinatario en el extranjero)
            _ => false
        };
    }
    
    /// <summary>
    /// Valida número de serie (F001, B001, etc.)
    /// </summary>
    public static bool ValidateSerie(string serie, string tipoDoc)
    {
        if (string.IsNullOrWhiteSpace(serie) || serie.Length != 4)
            return false;
        
        var prefix = serie[..1];
        var expectedPrefix = tipoDoc switch
        {
            "01" => "F", // Factura
            "03" => "B", // Boleta
            "07" => "F" or "B" or "FC" or "BC", // Nota de crédito
            "08" => "F" or "B" or "FD" or "BD", // Nota de débito
            "09" => "T" or "V", // Guía de remisión
            _ => true
        };
        
        if (expectedPrefix is string p)
            return prefix == p || serie.StartsWith(p);
        
        return true;
    }
    
    /// <summary>
    /// Valida correlativo (1-99999999)
    /// </summary>
    public static bool ValidateCorrelativo(int correlativo)
    {
        return correlativo > 0 && correlativo <= 99999999;
    }
}
