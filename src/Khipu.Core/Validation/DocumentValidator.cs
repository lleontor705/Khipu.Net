namespace Khipu.Core.Validation;

using Khipu.Core.Constants;

public static class DocumentValidator
{
    public static bool ValidateRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != SunatConstants.RucLength || !IsAsciiDigits(ruc))
        {
            return false;
        }

        var prefix = ruc[..2];
        if (!new[] { "10", "15", "17", "20" }.Contains(prefix, StringComparer.Ordinal))
        {
            return false;
        }

        var factors = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < factors.Length; i++)
        {
            sum += (ruc[i] - '0') * factors[i];
        }

        var remainder = sum % 11;
        var calculated = 11 - remainder;
        if (calculated == 10) calculated = 0;
        if (calculated == 11) calculated = 1;

        var given = ruc[10] - '0';
        return given == calculated;
    }

    public static bool ValidateDni(string dni)
        => !string.IsNullOrWhiteSpace(dni) && dni.Length == SunatConstants.DniLength && IsAsciiDigits(dni);

    public static bool ValidateDocument(string tipoDoc, string numDoc)
    {
        if (tipoDoc == "0")
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(numDoc))
        {
            return false;
        }

        return tipoDoc switch
        {
            "6" => ValidateRuc(numDoc),
            "1" => ValidateDni(numDoc),
            "4" => numDoc.Length <= 12,
            "7" => numDoc.Length <= 12,
            "A" => numDoc.Length <= 15,
            _ => false,
        };
    }

    public static bool ValidateSerie(string serie, string tipoDoc)
    {
        if (string.IsNullOrWhiteSpace(serie) || serie.Length != 4)
        {
            return false;
        }

        return tipoDoc switch
        {
            "01" => serie.StartsWith('F'),
            "03" => serie.StartsWith('B'),
            "07" => serie.StartsWith('F') || serie.StartsWith('B'),
            "08" => serie.StartsWith('F') || serie.StartsWith('B'),
            "09" => serie.StartsWith('T') || serie.StartsWith('V'),
            _ => true,
        };
    }

    public static bool ValidateCorrelativo(int correlativo)
        => correlativo > 0 && correlativo <= 99999999;

    private static bool IsAsciiDigits(string value)
        => value.All(static ch => ch is >= '0' and <= '9');
}
