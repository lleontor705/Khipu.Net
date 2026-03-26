namespace Khipu.Tests;

using Khipu.Core.Factory;
using Khipu.Core.Algorithms;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Xunit;

public class InvoiceFactoryTests
{
    private readonly InvoiceFactory _factory;
    private readonly Company _company;

    public InvoiceFactoryTests()
    {
        _company = new Company
        {
            Ruc = \"20123456789\",
            RazonSocial = \"EMPRESA SAC\"
        };
        _factory = new InvoiceFactory(_company);
    }

    [Fact]
    public void CreateInvoice_WithValidData_ReturnsInvoice()
    {
        var client = new Client
        {
            TipoDoc = DocumentType.Ruc,
            NumDoc = \"20987654321\",
            RznSocial = \"CLIENTE SRL\"
        };

        var invoice = _factory.CreateInvoice(client, \"F001\", 1, DateTime.Now);

        Assert.NotNull(invoice);
        Assert.Equal(\"F001\", invoice.Serie);
        Assert.Equal(1, invoice.Correlativo);
        Assert.Equal(VoucherType.Factura, invoice.TipoDoc);
        Assert.Equal(\"0101\", invoice.TipoOperacion);
    }

    [Fact]
    public void CreateDetail_WithValidData_ReturnsDetailWithCalculations()
    {
        var detail = _factory.CreateDetail(
            codigo: \"PROD001\",
            descripcion: \"Producto de prueba\",
            unidad: \"NIU\",
            cantidad: 2,
            valorUnitario: 100,
            tipoAfectacion: TaxType.Gravado
        );

        Assert.Equal(\"PROD001\", detail.Codigo);
        Assert.Equal(2, detail.Cantidad);
        Assert.Equal(100, detail.MtoValorUnitario);
        Assert.Equal(200, detail.MtoValorVenta);
        Assert.Equal(236, detail.PrecioVenta); // 200 + 18%
        Assert.Equal(TaxType.Gravado, detail.TipoAfectacionIgv);
    }

    [Fact]
    public void CreateDetail_Exonerado_ReturnsDetailWithoutIgv()
    {
        var detail = _factory.CreateDetail(
            codigo: \"PROD001\",
            descripcion: \"Producto exonerado\",
            unidad: \"NIU\",
            cantidad: 1,
            valorUnitario: 100,
            tipoAfectacion: TaxType.Exonerado
        );

        Assert.Equal(100, detail.MtoValorVenta);
        Assert.Equal(100, detail.PrecioVenta); // Sin IGV
        Assert.Equal(TaxType.Exonerado, detail.TipoAfectacionIgv);
    }

    [Fact]
    public void CreateCreditNote_WithValidData_ReturnsCreditNote()
    {
        var client = new Client
        {
            TipoDoc = DocumentType.Ruc,
            NumDoc = \"20987654321\",
            RznSocial = \"CLIENTE SRL\"
        };

        var note = _factory.CreateCreditNote(
            client,
            \"FC01\",
            1,
            DateTime.Now,
            \"F001-100\",
            \"01\",
            \"Anulación\"
        );

        Assert.NotNull(note);
        Assert.Equal(\"FC01\", note.Serie);
        Assert.Equal(VoucherType.NotaCredito, note.TipoDoc);
        Assert.Equal(\"F001-100\", note.NumDocAfectado);
        Assert.Equal(\"01\", note.CodMotivo);
    }
}
