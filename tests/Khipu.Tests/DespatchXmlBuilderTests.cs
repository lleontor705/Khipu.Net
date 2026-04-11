namespace Khipu.Tests;

using Khipu.Data.Common;
using Khipu.Data.Documents;
using Khipu.Data.Entities;
using Khipu.Data.Enums;
using Khipu.Xml.Builder;
using Xunit;

public class DespatchXmlBuilderTests
{
    [Fact]
    public void Build_WithValidDespatch_GeneratesValidXml()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.NotNull(xml);
        Assert.Contains("DespatchAdvice", xml);
        Assert.Contains("T001-1", xml);
        Assert.Contains("SUNAT_Envio", xml);
    }

    [Fact]
    public void Build_ContainsSupplierAndCustomerParty()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("DespatchSupplierParty", xml);
        Assert.Contains("DeliveryCustomerParty", xml);
        Assert.Contains("20123456789", xml);
        Assert.Contains("CLIENTE SRL", xml);
    }

    [Fact]
    public void Build_ContainsShipmentDetails()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("Shipment", xml);
        Assert.Contains("HandlingCode", xml);
        Assert.Contains("01", xml); // CodMotivoTraslado
        Assert.Contains("GrossWeightMeasure", xml);
        Assert.Contains("25.500", xml); // PesoTotal
    }

    [Fact]
    public void Build_ContainsTransportInfo()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("CarrierParty", xml);
        Assert.Contains("TRANSPORTES SAC", xml);
        Assert.Contains("DriverPerson", xml);
        Assert.Contains("CARLOS", xml);
        Assert.Contains("TransportHandlingUnit", xml);
        Assert.Contains("ABC-123", xml);
    }

    [Fact]
    public void Build_ContainsAddresses()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("DeliveryAddress", xml);
        Assert.Contains("OriginAddress", xml);
        Assert.Contains("150101", xml); // Ubigeo partida
        Assert.Contains("150201", xml); // Ubigeo llegada
    }

    [Fact]
    public void Build_ContainsDespatchLines()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("DespatchLine", xml);
        Assert.Contains("DeliveredQuantity", xml);
        Assert.Contains("Producto de prueba", xml);
        Assert.Contains("PROD001", xml);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFormat()
    {
        var despatch = CreateTestDespatch();
        var builder = new DespatchXmlBuilder();

        var fileName = builder.GetFileName(despatch);

        Assert.Equal("20123456789-09-T001-00000001.xml", fileName);
    }

    [Fact]
    public void Build_WithoutTransportista_OmitsCarrierParty()
    {
        var despatch = CreateTestDespatch();
        despatch.Transportista = null;
        despatch.Conductores = null;
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.DoesNotContain("CarrierParty", xml);
        Assert.DoesNotContain("DriverPerson", xml);
    }

    [Fact]
    public void Build_WithSecondaryVehicle_ContainsAttachedEquipment()
    {
        var despatch = CreateTestDespatch();
        despatch.Vehiculo!.NroPlacaRemolque = "XYZ-999";
        var builder = new DespatchXmlBuilder();

        var xml = builder.Build(despatch);

        Assert.Contains("AttachedTransportEquipment", xml);
        Assert.Contains("XYZ-999", xml);
    }

    private Despatch CreateTestDespatch()
    {
        return new Despatch
        {
            Company = new Company
            {
                Ruc = "20123456789",
                RazonSocial = "EMPRESA SAC",
                Address = new Address
                {
                    Ubigeo = "150101",
                    Departamento = "LIMA",
                    Provincia = "LIMA",
                    Distrito = "LIMA",
                    Direccion = "AV. PRINCIPAL 123",
                    CodigoLocal = "0000"
                }
            },
            Destinatario = new Client
            {
                TipoDoc = DocumentType.Ruc,
                NumDoc = "20987654321",
                RznSocial = "CLIENTE SRL"
            },
            Serie = "T001",
            Correlativo = 1,
            FechaEmision = new DateTime(2026, 4, 10),
            CodMotivoTraslado = "01", // Venta
            DesMotivoTraslado = "Venta de mercadería",
            IndTransbordo = "01", // Transporte público
            PesoTotal = 25.5m,
            UndPesoTotal = "KGM",
            NumBultos = 3,
            PuntoPartida = new Address
            {
                Ubigeo = "150101",
                Direccion = "AV. ORIGEN 456, LIMA"
            },
            PuntoLlegada = new Address
            {
                Ubigeo = "150201",
                Direccion = "JR. DESTINO 789, CALLAO"
            },
            Transportista = new Transportist
            {
                TipoDoc = "6",
                NumDoc = "20111222333",
                RznSocial = "TRANSPORTES SAC"
            },
            Vehiculo = new Vehicle
            {
                Placa = "ABC-123"
            },
            Conductores = new List<Driver>
            {
                new()
                {
                    TipoDoc = "1",
                    NumDoc = "12345678",
                    Nombres = "CARLOS",
                    Apellidos = "RAMIREZ LOPEZ",
                    Licencia = "Q12345678"
                }
            },
            Details = new List<DespatchDetail>
            {
                new()
                {
                    Orden = 1,
                    Codigo = "PROD001",
                    Descripcion = "Producto de prueba",
                    Unidad = "NIU",
                    Cantidad = 10,
                    CodProdSunat = "10191509"
                }
            }
        };
    }
}
