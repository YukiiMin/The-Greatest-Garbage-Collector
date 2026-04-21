using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload 1 ảnh lên Cloudinary, trả về URL công khai.
        /// </summary>
        Task<string> UploadImageAsync(IFormFile file, string folder = "waste-reports");

        /// <summary>
        /// Upload tối đa 3 ảnh lên Cloudinary song song, trả về danh sách URL.
        /// </summary>
        Task<List<string>> UploadImagesAsync(IList<IFormFile> files, string folder = "waste-reports");

        /// <summary>
        /// Xóa ảnh khỏi Cloudinary theo publicId.
        /// </summary>
        Task DeleteImageAsync(string publicId);

        /// <summary>
        /// Xóa nhiều ảnh theo danh sách URL Cloudinary.
        /// </summary>
        Task DeleteImagesAsync(IList<string> imageUrls);
    }
}
