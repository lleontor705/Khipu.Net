namespace Khipu.Ws.Constants;

/// <summary>
/// URLs de los servicios web de SUNAT
/// </summary>
public static class SunatEndpoints
{
    public const string Production = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService";
    public const string ProductionAlt = "https://e-guiaremision.sunat.gob.pe/ol-ti-itemision-guia-gem/billService";
    public const string ProductionConsult = "https://e-factura.sunat.gob.pe/ol-it-wsconsvalidcpe/billConsultService";
    
    public const string Beta = "https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService";
    public const string BetaAlt = "https://e-guiaremision.sunat.gob.pe/ol-ti-itemision-guia-gem-beta/billService";
    public const string BetaConsult = "https://e-factura.sunat.gob.pe/ol-it-wsconsvalidcpe-beta/billConsultService";
}
