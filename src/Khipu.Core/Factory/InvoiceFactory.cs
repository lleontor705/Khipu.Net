namespace Khipu.Core.Factory;

using Khipu.Core.Algorithms;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

public class InvoiceFactory
{
    private readonly Company _company;

    public InvoiceFactory(Company company)
    {
        _company = company ?? throw new ArgumentNullException(nameof(company));
    }

    public Invoice CreateInvoice(Client client, string serie, int correlativo, DateTime fechaEmision)
    {
        return new Invoice
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            TipoOperacion = "0101"
        };
    }

    public Receipt CreateReceipt(Client client, string serie, int correlativo, DateTime fechaEmision)
    {
        return new Receipt
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie),
            Correlativo = correlativo,
            FechaEmision = fechaEmision
        };
    }

    public CreditNote CreateCreditNote(Client client, string serie, int correlativo, DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
    {
        return new CreditNote
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            NumDocAfectado = docAfectado,
            CodMotivo = codMotivo,
            DesMotivo = desMotivo
        };
    }

    public DebitNote CreateDebitNote(Client client, string serie, int correlativo, DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
    {
        return new DebitNote
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            NumDocAfectado = docAfectado,
            CodMotivo = codMotivo,
            DesMotivo = desMotivo
        };
    }

    public SaleDetail CreateDetail(string codigo, string descripcion, string unidad, decimal cantidad, decimal valorUnitario, TaxType tipoAfectacion = TaxType.Gravado)
    {
        var valorVenta = RoundingPolicy.RoundSunat(cantidad * valorUnitario);
        var precioVenta = TaxCalculator.CalculateSalePrice(valorVenta, tipoAfectacion);

        return new SaleDetail
        {
            Codigo = codigo,
            Descripcion = descripcion,
            Unidad = unidad,
            Cantidad = cantidad,
            MtoValorUnitario = valorUnitario,
            MtoValorVenta = valorVenta,
            PrecioVenta = precioVenta,
            TipoAfectacionIgv = tipoAfectacion,
            TasaIgv = 0.18m
        };
    }

    private static string ValidateSerie(string serie)
    {
        if (string.IsNullOrWhiteSpace(serie))
        {
            throw new ArgumentException("Serie es requerida", nameof(serie));
        }

        if (serie.Length != 4)
        {
            throw new ArgumentException($"Serie debe tener 4 caracteres: {serie}", nameof(serie));
        }

        return serie;
    }
}
