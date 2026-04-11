namespace Khipu.Tests;

using Khipu.Core.Services;
using Xunit;

public class DocumentNumberServiceTests
{
    [Fact]
    public void GetNextCorrelativo_ReturnsIncrementing()
    {
        var service = new DocumentNumberService(0);
        Assert.Equal(1, service.GetNextCorrelativo());
        Assert.Equal(2, service.GetNextCorrelativo());
        Assert.Equal(3, service.GetNextCorrelativo());
    }

    [Fact]
    public void GetNextCorrelativo_WithInitialValue_StartsFromThere()
    {
        var service = new DocumentNumberService(100);
        Assert.Equal(101, service.GetNextCorrelativo());
    }

    [Fact]
    public void GenerateDocumentNumber_ReturnsCorrectFormat()
    {
        var service = new DocumentNumberService();
        Assert.Equal("F001-00000001", service.GenerateDocumentNumber("F001", 1));
        Assert.Equal("B001-00000123", service.GenerateDocumentNumber("B001", 123));
    }

    [Fact]
    public void GenerateFileName_ReturnsXmlFormat()
    {
        var service = new DocumentNumberService();
        Assert.Equal("20100070970-01-F001-00000001.xml", service.GenerateFileName("20100070970", "01", "F001", 1));
    }

    [Fact]
    public void GenerateZipName_ReturnsZipFormat()
    {
        var service = new DocumentNumberService();
        Assert.Equal("20100070970-01-F001-00000001.zip", service.GenerateZipName("20100070970", "01", "F001", 1));
    }
}
