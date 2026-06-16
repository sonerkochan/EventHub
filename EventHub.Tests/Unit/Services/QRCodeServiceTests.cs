using EventHub.Core.Services;

namespace EventHub.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class QRCodeServiceTests
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public void GenerateQRCode_WithValidPayload_ReturnsValidBase64Png()
    {
        var service = new QRCodeService();

        var result = service.GenerateQRCode("ticket:12345");

        Assert.False(string.IsNullOrWhiteSpace(result));

        var bytes = Convert.FromBase64String(result);
        Assert.True(bytes.Length > PngSignature.Length);
        Assert.Equal(PngSignature, bytes.Take(PngSignature.Length).ToArray());
    }

    [Fact]
    public void GenerateQRCode_SamePayloadAndSize_IsDeterministic()
    {
        var service = new QRCodeService();

        var first = service.GenerateQRCode("ticket:12345", 10);
        var second = service.GenerateQRCode("ticket:12345", 10);

        Assert.Equal(first, second);
    }

    [Fact]
    public void GenerateQRCode_LargerSize_ProducesLargerImage()
    {
        var service = new QRCodeService();

        var small = Convert.FromBase64String(service.GenerateQRCode("ticket:12345", 5));
        var large = Convert.FromBase64String(service.GenerateQRCode("ticket:12345", 20));

        Assert.True(large.Length > small.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateQRCode_NullOrWhitespacePayload_ThrowsArgumentException(string? payload)
    {
        var service = new QRCodeService();

        var ex = Assert.Throws<ArgumentException>(() => service.GenerateQRCode(payload!));
        Assert.Equal("payload", ex.ParamName);
    }
}
