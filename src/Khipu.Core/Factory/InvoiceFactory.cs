namespace Khipu.Core.Factory;

using Khipu.Core.Builder;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Factory para crear documentos SUNAT (basado en Greenter InvoiceFactory)
/// </summary>
public class InvoiceFactory
{
    private readonly Company _company;

    public InvoiceFactory(Company company)
    {
        _company = company ?? throw new ArgumentNullException(nameof(company));
    }

    /// <summary>
    /// Crea una factura
    /// </summary>
    public Invoice CreateInvoice(Client client, string serie, int correlativo, DateTime fechaEmision)
    {
        return new Invoice
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie, \"01\"),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            TipoOperacion = \"0101\" // Venta interna
        };
    }

    /// <summary>
    /// Crea una boleta
    /// </summary>
    public Receipt CreateReceipt(Client client, string serie, int correlativo, DateTime fechaEmision)
    {
        return new Receipt
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie, \"03\"),
            Correlativo = correlativo,
            FechaEmision = fechaEmision
        };
    }

    /// <summary>
    /// Crea una nota de crédito
    /// </summary>
    public CreditNote CreateCreditNote(Client client, string serie, int correlativo, 
        DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
    {
        return new CreditNote
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie, \"07\"),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            NumDocAfectado = docAfectado,
            CodMotivo = codMotivo,
            DesMotivo = desMotivo
        };
    }

    /// <summary>
    /// Crea una nota de débito
    /// </summary>
    public DebitNote CreateDebitNote(Client client, string serie, int correlativo,
        DateTime fechaEmision, string docAfectado, string codMotivo, string desMotivo)
    {
        return new DebitNote
        {
            Company = _company,
            Client = client ?? throw new ArgumentNullException(nameof(client)),
            Serie = ValidateSerie(serie, \"08\"),
            Correlativo = correlativo,
            FechaEmision = fechaEmision,
            NumDocAfectado = docAfectado,
            CodMotivo = codMotivo,
            DesMotivo = desMotivo
        };
    }

    /// <summary>
    /// Crea un detalle de venta
    /// </summary>
    public SaleDetail CreateDetail(string codigo, string descripcion, string unidad, 
        decimal cantidad, decimal valorUnitario, TaxType tipoAfectacion = TaxType.Gravado)
    {
        var valorVenta = Math.Round(cantidad * valorUnitario, 2);
        var igv = tipoAfectacion == TaxType.Gravado ? valorVenta * 0.18m : 0;
        var precioVenta = valorVenta + igv;

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

    private string ValidateSerie(string serie, string tipoDoc)
    {
        if (string.IsNullOrWhiteSpace(serie))
            throw new ArgumentException(\"Serie es requerida\");

        if (serie.Length != 4)
            throw new ArgumentException(\$\"Serie debe tener 4 caracteres: {serie}\");

        return serie;
    }
}
