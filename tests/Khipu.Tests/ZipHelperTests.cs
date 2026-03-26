namespace Khipu.Tests;

using Xunit;
using Khipu.Ws.Helpers;

public class ZipHelperTests
{
    [Fact]
    public void CreateZip_WithValidContent_ReturnsZipBytes()
    {
        var xml = "<test>content</test>";
        var result = ZipHelper.CreateZip("test.xml", xml);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}
