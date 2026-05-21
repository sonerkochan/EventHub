using Microsoft.AspNetCore.Http;

namespace EventHub.Core.Contracts
{
    public interface IPhotoService
    {
        Task<Guid?> UploadPhotoAsync(IFormFile file);
    }
}
