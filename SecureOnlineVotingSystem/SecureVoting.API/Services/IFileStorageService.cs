using Microsoft.AspNetCore.Http;

namespace SecureVoting.API.Services
{
    public interface IFileStorageService
    {
        Task<(bool ok, string message, string? fileName, string? relativePath)> SaveIdPictureAsync(IFormFile file);

        Task<(bool ok, string message, string? fileName, string? relativePath)> SaveCandidatePictureAsync(IFormFile file);
    }
}
