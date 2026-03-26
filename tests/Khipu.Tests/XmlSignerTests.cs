namespace Khipu.Tests;

using System.Security.Cryptography.X509Certificates;
using Khipu.Core.Security;
using Xunit;

public class XmlSignerTests
{
    [Fact]
    public void Constructor_WithValidCertificate_CreatesInstance()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        
        // Act
        var signer = new XmlSigner(cert);
        
        // Assert
        Assert.NotNull(signer);
    }

    [Fact]
    public void Constructor_WithNullCertificate_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new XmlSigner(null!));
    }

    [Fact]
    public void IsCertificateValid_WithValidCertificate_ReturnsTrue()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var signer = new XmlSigner(cert);
        
        // Act
        var isValid = signer.IsCertificateValid();
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void GetCertificateInfo_ReturnsCorrectInfo()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var signer = new XmlSigner(cert);
        
        // Act
        var info = signer.GetCertificateInfo();
        
        // Assert
        Assert.NotNull(info);
        Assert.True(info.HasPrivateKey);
        Assert.True(info.NotAfter > DateTime.Now);
    }

    [Fact]
    public void Sign_WithValidXml_ReturnsSignedXml()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var signer = new XmlSigner(cert);
        var xml = CreateTestXml();
        
        // Act
        var signedXml = signer.Sign(xml);
        
        // Assert
        Assert.NotNull(signedXml);
        Assert.Contains("Signature", signedXml);
    }

    [Fact]
    public void Sign_WithExtensionContent_InsertsSignatureInCorrectPlace()
    {
        // Arrange
        using var cert = CreateTestCertificate();
        var signer = new XmlSigner(cert);
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Invoice xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
    <ext:UBLExtensions xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
        <ext:UBLExtension>
            <ext:ExtensionContent></ext:ExtensionContent>
        </ext:UBLExtension>
    </ext:UBLExtensions>
</Invoice>";
        
        // Act
        var signedXml = signer.Sign(xml);
        
        // Assert
        Assert.Contains("ExtensionContent", signedXml);
        Assert.Contains("Signature", signedXml);
    }

    private X509Certificate2 CreateTestCertificate()
    {
        // Crear certificado de prueba auto-firmado
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Test Certificate,O=Test Organization,C=PE",
            rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);

        // Agregar usos del certificado
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                false));

        // Crear certificado válido por 1 año
        var certificate = request.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(1));

        return certificate;
    }

    private string CreateTestXml()
    {
        return @"<?xml version=""1.0"" encoding=""utf-8""?>
<Invoice xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
    <ID>F001-123</ID>
    <IssueDate>2026-03-26</IssueDate>
</Invoice>";
    }
}
