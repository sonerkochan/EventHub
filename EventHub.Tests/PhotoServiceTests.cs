using EventHub.Core.Services;

namespace EventHub.Tests
{
    public class PhotoServiceTests
    {
        [Fact]
        public void ValidatePhoto_AllowsSupportedImageTypes()
        {
            PhotoService.ValidatePhoto("image/jpeg", 1024);
            PhotoService.ValidatePhoto("image/png", 1024);
            PhotoService.ValidatePhoto("image/webp", 1024);
            PhotoService.ValidatePhoto("image/heic", 1024);
        }

        [Fact]
        public void ValidatePhoto_RejectsInvalidContentType()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => PhotoService.ValidatePhoto("text/plain", 1024));

            Assert.Equal("Invalid image type.", exception.Message);
        }

        [Fact]
        public void ValidatePhoto_RejectsFilesAboveFiveMegabytes()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => PhotoService.ValidatePhoto("image/png", PhotoService.MaxFileSize + 1));

            Assert.Equal("File size exceeds the 5MB limit.", exception.Message);
        }
    }
}
