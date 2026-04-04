namespace Khipu.Core.Security;

using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

/// <summary>
/// Excepción para errores de carga de certificado
/// </summary>
public class CertificateLoadException : Exception
{
    public CertificateLoadException(string message) : base(message) { }
    public CertificateLoadException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Excepción para errores de firma XML
/// </summary>
public class XmlSignException : Exception
{
    public XmlSignException(string message) : base(message) { }
    public XmlSignException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Firmador de documentos XML con certificados X.509
/// </summary>
public class XmlSigner
{
    private readonly X509Certificate2 _certificate;

    /// <summary>
    /// Constructor con certificado
    /// </summary>
    public XmlSigner(X509Certificate2 certificate)
    {
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        
        if (!certificate.HasPrivateKey)
            throw new CertificateLoadException("El certificado no tiene clave privada");
    }

    /// <summary>
    /// Carga certificado desde archivo PFX con validación completa
    /// </summary>
    public static XmlSigner FromPfx(string pfxPath, string password)
    {
        try
        {
            // Validar parámetros
            if (string.IsNullOrWhiteSpace(pfxPath))
                throw new ArgumentNullException(nameof(pfxPath), "Ruta del certificado es requerida");
            
            if (!File.Exists(pfxPath))
                throw new FileNotFoundException($"No se encontró el archivo de certificado: {pfxPath}");
            
            // Cargar certificado
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, password);
            
            if (certificate == null)
                throw new CertificateLoadException("No se pudo cargar el certificado");
            
            // Validar certificado
            if (!certificate.HasPrivateKey)
                throw new CertificateLoadException("El certificado no contiene clave privada");
            
            if (certificate.NotAfter < DateTime.UtcNow)
                throw new CertificateLoadException($"El certificado expiró el {certificate.NotAfter:dd/MM/yyyy}");
            
            if (certificate.NotBefore > DateTime.UtcNow)
                throw new CertificateLoadException($"El certificado no es válido hasta el {certificate.NotBefore:dd/MM/yyyy}");
            
            return new XmlSigner(certificate);
        }
        catch (CryptographicException ex)
        {
            throw new CertificateLoadException($"Error criptográfico al cargar certificado. Verifique la contraseña. {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not CertificateLoadException)
        {
            throw new CertificateLoadException($"Error inesperado al cargar certificado: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Firma un documento XML según estándar XMLDSig
    /// </summary>
    public string Sign(string xmlContent)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(xmlContent))
                throw new ArgumentNullException(nameof(xmlContent), "Contenido XML es requerido");
            
            // Cargar XML
            var doc = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
            doc.LoadXml(xmlContent);

            // Crear objeto SignedXml
            var signedXml = new SignedXml(doc)
            {
                SigningKey = _certificate.GetRSAPrivateKey()
            };
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            // Configurar referencia
            var reference = new Reference
            {
                Uri = "",
                DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
            };

            // Agregar transformaciones
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());

            signedXml.AddReference(reference);

            // Configurar KeyInfo
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(_certificate));
            signedXml.KeyInfo = keyInfo;

            // Firmar
            signedXml.ComputeSignature();

            // Obtener elemento de firma
            var xmlDigitalSignature = signedXml.GetXml();

            // Buscar el nodo ExtensionContent donde va la firma
            var extensionContent = doc.SelectSingleNode("//*[local-name()='ExtensionContent']");
            if (extensionContent != null)
            {
                extensionContent.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            }
            else
            {
                // Si no existe ExtensionContent, agregar al final del documento
                doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            }

            return doc.OuterXml;
        }
        catch (XmlException ex)
        {
            throw new XmlSignException($"Error al procesar XML: {ex.Message}", ex);
        }
        catch (CryptographicException ex)
        {
            throw new XmlSignException($"Error criptográfico al firmar: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not XmlSignException)
        {
            throw new XmlSignException($"Error inesperado al firmar XML: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Verifica si un certificado es válido
    /// </summary>
    public bool IsCertificateValid()
    {
        return _certificate != null && 
               _certificate.HasPrivateKey && 
               _certificate.NotAfter > DateTime.UtcNow &&
               _certificate.NotBefore <= DateTime.UtcNow;
    }

    /// <summary>
    /// Obtiene información del certificado
    /// </summary>
    public CertificateInfo GetCertificateInfo()
    {
        return new CertificateInfo
        {
            Subject = _certificate.Subject,
            Issuer = _certificate.Issuer,
            NotBefore = _certificate.NotBefore,
            NotAfter = _certificate.NotAfter,
            Thumbprint = _certificate.Thumbprint,
            HasPrivateKey = _certificate.HasPrivateKey,
            IsValid = IsCertificateValid()
        };
    }
}

/// <summary>
/// Información del certificado
/// </summary>
public class CertificateInfo
{
    public string Subject { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Thumbprint { get; set; } = string.Empty;
    public bool HasPrivateKey { get; set; }
    public bool IsValid { get; set; }
}
