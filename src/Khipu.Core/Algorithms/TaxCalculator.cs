namespace Khipu.Core.Algorithms;

using Khipu.Core.Constants;
using Khipu.Data.Enums;

public static class TaxCalculator
{
    public static decimal CalculateIgv(decimal taxableAmount)
    {
        if (taxableAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taxableAmount), "Taxable amount cannot be negative");
        }

        return RoundingPolicy.RoundSunat(taxableAmount * SunatConstants.TasaIGV);
    }

    public static decimal CalculateSalePrice(decimal baseAmount, TaxType taxType)
    {
        if (baseAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseAmount), "Base amount cannot be negative");
        }

        if (taxType == TaxType.Gravado)
        {
            return RoundingPolicy.RoundSunat(baseAmount + CalculateIgv(baseAmount));
        }

        return RoundingPolicy.RoundSunat(baseAmount);
    }

    public static decimal CalculateUnitValue(decimal salePrice, TaxType taxType)
    {
        if (salePrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(salePrice), "Sale price cannot be negative");
        }

        if (taxType != TaxType.Gravado)
        {
            return RoundingPolicy.RoundSunat(salePrice);
        }

        var value = salePrice / (1 + SunatConstants.TasaIGV);
        return RoundingPolicy.RoundSunat(value);
    }

    public static decimal CalculateDetraction(decimal baseAmount, decimal rate = SunatConstants.TasaDetraccion)
    {
        if (baseAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseAmount), "Base amount cannot be negative");
        }

        if (rate < 0 || rate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be in [0,1]");
        }

        return RoundingPolicy.RoundSunat(baseAmount * rate);
    }
}
