using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Upload tối đa 3 ảnh lên Cloudinary, trả về danh sách URL.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] IList<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn ít nhất 1 ảnh.", "VALIDATION_ERROR"));

            if (images.Count > 3)
                return BadRequest(ApiResponse<object>.Fail("Tối đa 3 ảnh mỗi lần upload.", "VALIDATION_ERROR"));

            var urls = await _cloudinaryService.UploadImagesAsync(images, "waste-reports");
            return Ok(ApiResponse<List<string>>.Ok(urls));
        }
    }
}
