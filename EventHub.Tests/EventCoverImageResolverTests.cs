using EventHub.Core.Services;

namespace EventHub.Tests
{
    public class EventCoverImageResolverTests
    {
        [Fact]
        public void BuildDisplayUrl_PrefersUploadedPhoto()
        {
            var photoId = Guid.NewGuid();

            var result = EventCoverImageResolver.BuildDisplayUrl(photoId, "https://example.com/cover.jpg");

            Assert.Equal($"/photos/{photoId}", result);
        }

        [Fact]
        public void BuildDisplayUrl_FallsBackToExternalUrl()
        {
            var result = EventCoverImageResolver.BuildDisplayUrl(null, " https://example.com/cover.jpg ");

            Assert.Equal("https://example.com/cover.jpg", result);
        }

        [Theory]
        [InlineData("https://example.com/cover.jpg", true)]
        [InlineData("http://example.com/cover.jpg", true)]
        [InlineData("", true)]
        [InlineData("ftp://example.com/cover.jpg", false)]
        [InlineData("/local/path.jpg", false)]
        public void IsValidExternalUrl_ValidatesAbsoluteHttpUrls(string value, bool expected)
        {
            Assert.Equal(expected, EventCoverImageResolver.IsValidExternalUrl(value));
        }
    }
}
