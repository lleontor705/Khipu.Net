namespace Khipu.Core.Algorithms;

using Khipu.Data.Enums;

public static class AmountInWordsEsPe
{
    public static string Convert(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");
        }

        var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        var integerPart = (long)Math.Truncate(rounded);
        var cents = (int)((rounded - integerPart) * 100);

        var currencyWord = currency switch
        {
            Currency.Pen => "SOLES",
            Currency.Usd => "DÓLARES AMERICANOS",
            _ => throw new NotSupportedException($"Currency {currency} is not supported")
        };

        return $"SON: {ToWords(integerPart)} CON {cents:00}/100 {currencyWord}";
    }

    private static string ToWords(long number)
    {
        if (number == 0) return "CERO";
        if (number < 0) return "MENOS " + ToWords(Math.Abs(number));

        return number switch
        {
            <= 15 => Units[number],
            < 20 => "DIECI" + ToWords(number - 10).ToLowerInvariant(),
            20 => "VEINTE",
            < 30 => "VEINTI" + ToWords(number - 20).ToLowerInvariant(),
            < 100 => BuildTens(number),
            100 => "CIEN",
            < 200 => "CIENTO " + ToWords(number - 100),
            < 1000 => BuildHundreds(number),
            1000 => "MIL",
            < 2000 => "MIL " + ToWords(number - 1000),
            < 1000000 => BuildThousands(number),
            1000000 => "UN MILLÓN",
            < 2000000 => "UN MILLÓN " + ToWords(number - 1000000),
            < 1000000000000 => BuildMillions(number),
            _ => throw new NotSupportedException("Amount too large")
        };
    }

    private static string BuildTens(long number)
    {
        var tens = number / 10;
        var units = number % 10;
        return units == 0 ? Tens[tens] : $"{Tens[tens]} Y {ToWords(units)}";
    }

    private static string BuildHundreds(long number)
    {
        var hundreds = number / 100;
        var rest = number % 100;
        return rest == 0 ? Hundreds[hundreds] : $"{Hundreds[hundreds]} {ToWords(rest)}";
    }

    private static string BuildThousands(long number)
    {
        var thousands = number / 1000;
        var rest = number % 1000;
        var thousandsText = thousands == 1 ? "MIL" : $"{ToWords(thousands)} MIL";
        return rest == 0 ? thousandsText : $"{thousandsText} {ToWords(rest)}";
    }

    private static string BuildMillions(long number)
    {
        var millions = number / 1000000;
        var rest = number % 1000000;
        var millionsText = millions == 1 ? "UN MILLÓN" : $"{ToWords(millions)} MILLONES";
        return rest == 0 ? millionsText : $"{millionsText} {ToWords(rest)}";
    }

    private static readonly Dictionary<long, string> Units = new()
    {
        [0] = "CERO", [1] = "UNO", [2] = "DOS", [3] = "TRES", [4] = "CUATRO", [5] = "CINCO",
        [6] = "SEIS", [7] = "SIETE", [8] = "OCHO", [9] = "NUEVE", [10] = "DIEZ",
        [11] = "ONCE", [12] = "DOCE", [13] = "TRECE", [14] = "CATORCE", [15] = "QUINCE"
    };

    private static readonly Dictionary<long, string> Tens = new()
    {
        [3] = "TREINTA", [4] = "CUARENTA", [5] = "CINCUENTA", [6] = "SESENTA",
        [7] = "SETENTA", [8] = "OCHENTA", [9] = "NOVENTA"
    };

    private static readonly Dictionary<long, string> Hundreds = new()
    {
        [1] = "CIENTO", [2] = "DOSCIENTOS", [3] = "TRESCIENTOS", [4] = "CUATROCIENTOS",
        [5] = "QUINIENTOS", [6] = "SEISCIENTOS", [7] = "SETECIENTOS", [8] = "OCHOCIENTOS", [9] = "NOVECIENTOS"
    };
}
