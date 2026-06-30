using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SmartApart.API.Common;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                ValidateFile(file);

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"smartapart/{folder}",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
                    throw new AppException("Image upload failed.", 500);
                }

                return result.SecureUrl.ToString();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadImageAsync failed for file {FileName}", file.FileName);
                throw new AppException("Image upload failed. Please try again.", 500);
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                var publicId = ExtractPublicId(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                    return;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result != "ok" && result.Result != "not found")
                {
                    _logger.LogWarning("Cloudinary delete returned: {Result} for {PublicId}", result.Result, publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteImageAsync failed for {ImageUrl}", imageUrl);
                // Don't throw — image cleanup failure shouldn't block the main operation
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new AppException("No file uploaded.", 400);

            if (file.Length > MaxFileSizeBytes)
                throw new AppException("File size cannot exceed 5MB.", 400);

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLower()))
                throw new AppException("Only JPEG, PNG, and WEBP images are allowed.", 400);
        }

        private static string? ExtractPublicId(string imageUrl)
        {
            // e.g. https://res.cloudinary.com/<cloud>/image/upload/v1234567/smartapart/maintenance/filename.jpg
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
                    return null;

                // Skip version segment (e.g. v1234567)
                var pathSegments = segments.Skip(uploadIndex + 2);
                var fullPath = string.Join('/', pathSegments);

                // Remove file extension
                var lastDot = fullPath.LastIndexOf('.');
                return lastDot > 0 ? fullPath[..lastDot] : fullPath;
            }
            catch
            {
                return null;
            }
        }
    }

}
