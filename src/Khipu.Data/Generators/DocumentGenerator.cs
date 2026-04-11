namespace Khipu.Data.Generators;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;

/// <summary>
/// Generador de documentos de prueba - Paridad Greenter data package.
/// Genera documentos electrónicos con datos de ejemplo para testing y demos.
/// </summary>
public static class DocumentGenerator
{
    private static readonly Company DefaultCompany = new()
    {
        Ruc = "20100070970",
        RazonSocial = "EMPRESA DE PRUEBA SAC",
        NombreComercial = "EMPRESA PRUEBA",
        Address = new Address
        {
            Ubigeo = "150101",
            Departamento = "LIMA",
            Provincia = "LIMA",
            Distrito = "LIMA",
            Direccion = "AV. LOS OLIVOS 123",
            CodigoLocal = "0000"
        }
    };

    private static readonly Client DefaultClient = new()
    {
        TipoDoc = DocumentType.Ruc,
        NumDoc = "20000000001",
        RznSocial = "CLIENTE EJEMPLO SRL",
        Address = new Address
        {
            Ubigeo = "150101",
            Direccion = "JR. COMERCIO 456, LIMA"
        }
    };

    public static Invoice CreateInvoice(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Client = DefaultClient,
        Serie = "F001",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        MtoOperGravadas = 1000.00m,
        MtoIGV = 180.00m,
        MtoImpVenta = 1180.00m,
        TotalImpuestos = 180.00m,
        Leyendas = new List<Legend>
        {
            new() { Code = "1000", Value = "MIL CIENTO OCHENTA CON 00/100 SOLES" }
        },
        Details = new List<SaleDetail>
        {
            new()
            {
                Codigo = "P001", Descripcion = "Producto de prueba 1",
                Unidad = "NIU", Cantidad = 2, TipoAfectacionIgv = TaxType.Gravado,
                MtoValorUnitario = 300.00m, MtoValorVenta = 600.00m,
                PrecioVenta = 354.00m, Igv = 108.00m
            },
            new()
            {
                Codigo = "S001", Descripcion = "Servicio de prueba 1",
                Unidad = "ZZ", Cantidad = 1, TipoAfectacionIgv = TaxType.Gravado,
                MtoValorUnitario = 400.00m, MtoValorVenta = 400.00m,
                PrecioVenta = 472.00m, Igv = 72.00m
            }
        }
    };

    public static Receipt CreateReceipt(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Client = new Client
        {
            TipoDoc = DocumentType.Dni,
            NumDoc = "12345678",
            RznSocial = "JUAN PEREZ GARCIA"
        },
        Serie = "B001",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        MtoOperGravadas = 100.00m,
        MtoIGV = 18.00m,
        MtoImpVenta = 118.00m,
        Details = new List<SaleDetail>
        {
            new()
            {
                Codigo = "P001", Descripcion = "Producto",
                Unidad = "NIU", Cantidad = 1,
                MtoValorUnitario = 100.00m, MtoValorVenta = 100.00m, PrecioVenta = 118.00m
            }
        }
    };

    public static CreditNote CreateCreditNote(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Client = DefaultClient,
        Serie = "FC01",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        TipDocAfectado = "01",
        NumDocAfectado = "F001-00000001",
        CodMotivo = "01",
        DesMotivo = "Anulacion de la operacion",
        MtoOperGravadas = 500.00m,
        MtoIGV = 90.00m,
        MtoImpVenta = 590.00m,
        Details = new List<SaleDetail>
        {
            new()
            {
                Codigo = "P001", Descripcion = "Producto devuelto",
                Unidad = "NIU", Cantidad = 1,
                MtoValorUnitario = 500.00m, MtoValorVenta = 500.00m, PrecioVenta = 590.00m
            }
        }
    };

    public static DebitNote CreateDebitNote(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Client = DefaultClient,
        Serie = "FD01",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        TipDocAfectado = "01",
        NumDocAfectado = "F001-00000001",
        CodMotivo = "02",
        DesMotivo = "Aumento en el valor",
        MtoOperGravadas = 200.00m,
        MtoIGV = 36.00m,
        MtoImpVenta = 236.00m,
        Details = new List<SaleDetail>
        {
            new()
            {
                Codigo = "P001", Descripcion = "Ajuste de precio",
                Unidad = "NIU", Cantidad = 1,
                MtoValorUnitario = 200.00m, MtoValorVenta = 200.00m, PrecioVenta = 236.00m
            }
        }
    };

    public static Despatch CreateDespatch(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Destinatario = DefaultClient,
        Serie = "T001",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        CodMotivoTraslado = "01",
        DesMotivoTraslado = "Venta",
        IndTransbordo = "01",
        PesoTotal = 50.000m,
        UndPesoTotal = "KGM",
        NumBultos = 5,
        PuntoPartida = new Address { Ubigeo = "150101", Direccion = "AV. LOS OLIVOS 123, LIMA" },
        PuntoLlegada = new Address { Ubigeo = "040101", Direccion = "CALLE COMERCIO 789, AREQUIPA" },
        Transportista = new Transportist
        {
            TipoDoc = "6", NumDoc = "20600000001", RznSocial = "TRANSPORTES RAPIDO SAC"
        },
        Vehiculo = new Vehicle { Placa = "ABC-123" },
        Conductores = new List<Driver>
        {
            new() { TipoDoc = "1", NumDoc = "12345678", Nombres = "CARLOS", Apellidos = "RAMIREZ", Licencia = "Q12345678" }
        },
        Details = new List<DespatchDetail>
        {
            new() { Orden = 1, Codigo = "P001", Descripcion = "Producto 1", Unidad = "NIU", Cantidad = 10, CodProdSunat = "10191509" },
            new() { Orden = 2, Codigo = "P002", Descripcion = "Producto 2", Unidad = "KGM", Cantidad = 25.5m }
        }
    };

    public static Perception CreatePerception(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Proveedor = DefaultClient,
        Serie = "P001",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        MtoPercepcion = 23.60m,
        MtoTotal = 1180.00m,
        MtoTotalCobrar = 1203.60m,
        Details = new List<PerceptionDetail>
        {
            new()
            {
                Orden = 1, TipoDoc = "01", NumDoc = "F001-00000001",
                FechaEmision = DateTime.Today.AddDays(-5),
                CodMoneda = "PEN", ImpTotal = 1180.00m, ImpCobrar = 1180.00m,
                CodReg = "01", Porcentaje = 2.00m, MtoBase = 1180.00m, Mto = 23.60m
            }
        }
    };

    public static Retention CreateRetention(int correlativo = 1) => new()
    {
        Company = DefaultCompany,
        Proveedor = DefaultClient,
        Serie = "R001",
        Correlativo = correlativo,
        FechaEmision = DateTime.Today,
        Moneda = Currency.Pen,
        MtoRetencion = 35.40m,
        MtoTotal = 1180.00m,
        Details = new List<RetentionDetail>
        {
            new()
            {
                Orden = 1, TipoDoc = "01", NumDoc = "F001-00000001",
                FechaEmision = DateTime.Today.AddDays(-5),
                FechaPago = DateTime.Today,
                CodMoneda = "PEN", ImpTotal = 1180.00m, ImpPagar = 1144.60m,
                Pagos = new List<Payment>
                {
                    new() { FormaPago = "001", Monto = 1180.00m }
                }
            }
        }
    };

    public static Summary CreateSummary() => new()
    {
        Company = DefaultCompany,
        Correlativo = "001",
        FechaGeneracion = DateTime.Today,
        FechaEnvio = DateTime.Today,
        Details = new List<SummaryDetail>
        {
            new()
            {
                Orden = 1, TipoDoc = VoucherType.Boleta,
                SerieNro = "B001-00000001", Estado = "1",
                ClienteTipoDoc = "1", ClienteNroDoc = "12345678",
                Total = 118.00m, MtoOperGravadas = 100.00m, MtoIGV = 18.00m
            }
        }
    };

    public static Voided CreateVoided() => new()
    {
        Company = DefaultCompany,
        Correlativo = "001",
        FechaGeneracion = DateTime.Today,
        FechaEnvio = DateTime.Today,
        Details = new List<VoidedDetail>
        {
            new()
            {
                Orden = 1, TipoDoc = "01", SerieNro = "F001-00000001",
                FechaDoc = DateTime.Today.AddDays(-1),
                MotivoBaja = "Error en documento"
            }
        }
    };
}
