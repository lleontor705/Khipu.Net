namespace Khipu.Ws.Constants;

using System.Reflection;
using System.Xml.Linq;

/// <summary>
/// Códigos de error SUNAT - Paridad 100% Greenter xcodes package.
/// Carga los 1710 códigos de error desde el recurso embebido CodeErrors.xml.
/// Fuente: Catálogo de errores SUNAT para facturación electrónica.
/// </summary>
public static class SunatErrorCodes
{
    private static readonly Lazy<Dictionary<string, string>> _codes = new(LoadCodes);

    private static Dictionary<string, string> Codes => _codes.Value;

    /// <summary>
    /// Obtiene el mensaje de error para un código SUNAT.
    /// </summary>
    public static string? GetMessage(string? code)
    {
        if (string.IsNullOrEmpty(code)) return null;
        return Codes.TryGetValue(code, out var message) ? message : null;
    }

    /// <summary>
    /// Obtiene el mensaje de error, o un mensaje por defecto si el código no existe.
    /// </summary>
    public static string GetMessageOrDefault(string? code, string defaultMessage = "Código de error desconocido")
    {
        return GetMessage(code) ?? defaultMessage;
    }

    /// <summary>
    /// Verifica si un código corresponde a una observación (>= 4000) en vez de un rechazo.
    /// </summary>
    public static bool IsObservation(string? code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        return int.TryParse(code, out var num) && num >= 4000;
    }

    /// <summary>
    /// Verifica si un código corresponde a un error de rechazo (< 4000, excluyendo 0).
    /// </summary>
    public static bool IsRejection(string? code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        if (code == "0") return false;
        return int.TryParse(code, out var num) && num < 4000;
    }

    /// <summary>
    /// Verifica si el código indica aceptación (0 o >= 4000).
    /// Paridad con Greenter CdrResponse::isAccepted()
    /// </summary>
    public static bool IsAccepted(string? code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        if (code == "0") return true;
        return int.TryParse(code, out var num) && num >= 4000;
    }

    /// <summary>
    /// Obtiene la categoría del error según el rango del código.
    /// </summary>
    public static string GetCategory(string? code)
    {
        if (string.IsNullOrEmpty(code) || !int.TryParse(code, out var num))
            return "Desconocido";

        return num switch
        {
            0 => "Aceptado",
            >= 100 and < 151 => "Autenticación/Proceso",
            >= 151 and < 200 => "Archivo ZIP",
            >= 200 and < 300 => "Procesamiento",
            >= 300 and < 400 => "XML/Parsing",
            >= 400 and < 500 => "Validación Emisor",
            >= 1000 and < 1021 => "Validación Factura",
            >= 1021 and < 1032 => "Validación Nota Crédito/Débito",
            >= 1032 and < 1050 => "Validación Factura",
            >= 1050 and < 1083 => "Validación Guía Remisión",
            >= 2000 and < 2100 => "Validación Impuestos/Firma",
            >= 2100 and < 2200 => "Validación Detalle/Línea",
            >= 2200 and < 2300 => "Validación Percepción/Retención",
            >= 2300 and < 2400 => "Validación Resumen/Baja",
            >= 2400 and < 2500 => "Validación Guía Remisión",
            >= 2500 and < 3300 => "Validación Campos",
            >= 4000 => "Observación (Aceptado)",
            _ => "Otro"
        };
    }

    /// <summary>
    /// Retorna todos los códigos disponibles (1710 códigos).
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetAll() => Codes;

    /// <summary>
    /// Retorna la cantidad total de códigos cargados.
    /// </summary>
    public static int Count => Codes.Count;

    private static Dictionary<string, string> LoadCodes()
    {
        var dict = new Dictionary<string, string>(1800, StringComparer.Ordinal);

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("CodeErrors.xml", StringComparison.OrdinalIgnoreCase));

            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    var doc = XDocument.Load(stream);
                    if (doc.Root == null) return dict;
                    foreach (var error in doc.Root.Elements("error"))
                    {
                        var code = error.Attribute("code")?.Value;
                        var message = error.Value?.Trim();
                        if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(message))
                        {
                            dict[code] = message;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Khipu.Ws] Error loading SUNAT error codes: {ex.Message}");
        }

        // Si no se cargó nada, incluir al menos los códigos más críticos como fallback
        if (dict.Count == 0)
        {
            dict["0100"] = "El sistema no puede responder su solicitud";
            dict["0102"] = "Usuario o contraseña incorrectos";
            dict["0306"] = "No se puede leer (parsear) el archivo XML";
            dict["0307"] = "La firma digital es inválida";
        }

        return dict;
    }
}
