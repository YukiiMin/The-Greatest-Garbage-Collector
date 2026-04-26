using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IUploadImageService _uploadImageService;

        public ImageController(IUploadImageService uploadImageService)
        {
            _uploadImageService = uploadImageService;
        }

        /// <summary>
        /// Upload ảnh lên Cloudinary, trả về danh sách URL.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] IList<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn ít nhất 1 ảnh.", "VALIDATION_ERROR"));

            var urls = await _uploadImageService.UploadImagesAsync(images, "waste-reports");
            return Ok(ApiResponse<List<string>>.Ok(urls));
        }
    }
}
