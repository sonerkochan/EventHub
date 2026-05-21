using EventHub.Core.Contracts;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;

namespace EventHub.Core.Services
{
    public class PhotoService(
        ApplicationDbContext _context
        ) : IPhotoService
    {
        public const long MaxFileSize = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/heic"
        };

        public async Task<Guid?> UploadPhotoAsync(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                return null;
            }

            ValidatePhoto(file.ContentType, file.Length);

            using var ms = new MemoryStream();

            await file.CopyToAsync(ms);

            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Data = ms.ToArray(),
                UploadedAt = DateTime.UtcNow
            };

            _context.Add(photo);
            await _context.SaveChangesAsync();

            return photo.Id;

        }

        public static void ValidatePhoto(string? contentType, long length)
        {
            if (length > MaxFileSize)
            {
                throw new InvalidOperationException("File size exceeds the 5MB limit.");
            }

            if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException("Invalid image type.");
            }
        }
    }
}
