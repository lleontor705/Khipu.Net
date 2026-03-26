namespace Khipu.Core.Validation;

using Khipu.Core.Constants;

/// <summary>
/// Validador de documentos de identidad
/// </summary>
public static class DocumentValidator
{
    public static bool ValidateRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != SunatConstants.RucLength)
            return false;
        if (!ruc.All(char.IsDigit))
            return false;
        var prefix = ruc[..2];
        if (!new[] { "10", "15", "17", "20" }.Contains(prefix))
            return false;
        // Validación básica por ahora - TODO: implementar algoritmo completo
        return true;
    }
    
    public static bool ValidateDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni) || dni.Length != SunatConstants.DniLength)
            return false;
        return dni.All(char.IsDigit);
    }
    
    public static bool ValidateDocument(string tipoDoc, string numDoc)
    {
        return tipoDoc switch
        {
            "6" => ValidateRuc(numDoc),
            "1" => ValidateDni(numDoc),
            "4" or "7" or "A" => !string.IsNullOrWhiteSpace(numDoc),
            "0" => true,
            _ => false
        };
    }
}
