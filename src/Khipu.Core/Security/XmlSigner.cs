namespace Khipu.Core.Security;

using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

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
    }

    /// <summary>
    /// Carga certificado desde archivo PFX
    /// </summary>
    public static XmlSigner FromPfx(string pfxPath, string password)
    {
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, password);
        return new XmlSigner(certificate);
    }

    /// <summary>
    /// Firma un documento XML según estándar XMLDSig
    /// </summary>
    public string Sign(string xmlContent)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.LoadXml(xmlContent);

        // Crear objeto SignedXml
        var signedXml = new SignedXml(doc)
        {
            SigningKey = _certificate.GetRSAPrivateKey()
        };

        // Configurar referencia
        var reference = new Reference
        {
            Uri = ""
        };

        // Agregar transformación envolvente
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

    /// <summary>
    /// Verifica si un certificado es válido
    /// </summary>
    public bool IsCertificateValid()
    {
        return _certificate != null && 
               _certificate.HasPrivateKey && 
               _certificate.NotAfter > DateTime.Now &&
               _certificate.NotBefore <= DateTime.Now;
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
            HasPrivateKey = _certificate.HasPrivateKey
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
}
