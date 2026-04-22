using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using GarbageCollection.Common.Settings;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class UploadImageService : IUploadImageService
    {
        private readonly Cloudinary _cloudinary;

        public UploadImageService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "waste-reports")
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
                    .Width(1200)
                    .Crop("limit")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Upload thất bại: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        public async Task<List<string>> UploadImagesAsync(IList<IFormFile> files, string folder = "waste-reports")
        {
            var uploadTasks = files.Select(f => UploadImageAsync(f, folder));
            var urls = await Task.WhenAll(uploadTasks);
            return [.. urls];
        }

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }

        public async Task DeleteImagesAsync(IList<string> imageUrls)
        {
            if (imageUrls == null || imageUrls.Count == 0) return;

            var deleteTasks = imageUrls.Select(url =>
            {
                var publicId = ExtractPublicId(url);
                return DeleteImageAsync(publicId);
            });

            await Task.WhenAll(deleteTasks);
        }

        // Trích publicId từ URL Cloudinary
        // VD: https://res.cloudinary.com/demo/image/upload/v1234/waste-reports/abc.jpg
        //   → waste-reports/abc
        private static string ExtractPublicId(string url)
        {
            var uploadIndex = url.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex < 0) return url;

            var afterUpload = url[(uploadIndex + 8)..]; // bỏ "/upload/"

            // Bỏ version segment nếu có (vNNNN/)
            if (afterUpload.StartsWith('v') && afterUpload.Contains('/'))
            {
                var slash = afterUpload.IndexOf('/');
                if (afterUpload[1..slash].All(char.IsDigit))
                    afterUpload = afterUpload[(slash + 1)..];
            }

            // Bỏ extension
            var dotIndex = afterUpload.LastIndexOf('.');
            return dotIndex >= 0 ? afterUpload[..dotIndex] : afterUpload;
        }
    }
}
