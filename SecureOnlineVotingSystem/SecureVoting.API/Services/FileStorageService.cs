using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace SecureVoting.API.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(bool ok, string message, string? fileName, string? relativePath)> SaveIdPictureAsync(IFormFile file)
        {
            return await SaveImageAsync(file, "IdPictures", "ID picture");
        }

        public async Task<(bool ok, string message, string? fileName, string? relativePath)> SaveCandidatePictureAsync(IFormFile file)
        {
            return await SaveImageAsync(file, "Candidates", "Candidate picture");
        }

        private async Task<(bool ok, string message, string? fileName, string? relativePath)> SaveImageAsync(
            IFormFile file,
            string folderName,
            string displayName)
        {
            if (file == null || file.Length == 0)
                return (false, $"{displayName} is required.", null, null);

            if (file.Length > 5 * 1024 * 1024)
                return (false, $"{displayName} must be 5 MB or smaller.", null, null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return (false, "Only JPG, JPEG, PNG, and WEBP files are allowed.", null, null);

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "Uploads", folderName);

            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            var relativePath = $"/Uploads/{folderName}/{uniqueFileName}";

            return (true, "File uploaded successfully.", uniqueFileName, relativePath);
        }
    }
}