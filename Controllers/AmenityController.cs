using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Amenities;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly ICloudinaryService _cloudinaryService;

        public AmenityController(IAmenityService amenityService, ICloudinaryService cloudinaryService)
        {
            _amenityService = amenityService;
            _cloudinaryService = cloudinaryService;
        }

        // POST api/amenities
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityRequestDto dto)
        {
            var result = await _amenityService.CreateAmenityAsync(dto);
            return Created($"api/amenities/{result.Id}", ApiResponse<AmenityDetailDto>.Ok(result, "Amenity created successfully."));
        }

        // POST api/amenities/{amenityId}/images
        [HttpPost("{amenityId:guid}/images")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(Guid amenityId, IFormFile file)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "amenities");
            var result = await _amenityService.AddImageAsync(amenityId, imageUrl);
            return Created($"api/amenities/{amenityId}/images/{result.Id}", ApiResponse<AmenityImageDto>.Ok(result, "Image uploaded successfully."));
        }

        // GET api/amenities
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _amenityService.GetAllAsync();
            return Ok(ApiResponse<List<AmenityListDto>>.Ok(result));
        }

        // GET api/amenities/{amenityId}
        [HttpGet("{amenityId:guid}")]
        public async Task<IActionResult> GetById(Guid amenityId)
        {
            var result = await _amenityService.GetByIdAsync(amenityId);
            return Ok(ApiResponse<AmenityDetailDto>.Ok(result));
        }

        // PUT api/amenities/{amenityId}
        [HttpPut("{amenityId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAmenity(Guid amenityId, [FromBody] UpdateAmenityRequestDto dto)
        {
            var result = await _amenityService.UpdateAmenityAsync(amenityId, dto);
            return Ok(ApiResponse<AmenityDetailDto>.Ok(result, "Amenity updated successfully."));
        }

        // PATCH api/amenities/{amenityId}/availability
        [HttpPatch("{amenityId:guid}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAvailability(Guid amenityId, [FromBody] UpdateAmenityAvailabilityRequestDto dto)
        {
            var result = await _amenityService.UpdateAvailabilityAsync(amenityId, dto);
            return Ok(ApiResponse<AmenityDetailDto>.Ok(result, "Amenity availability updated successfully."));
        }

        // DELETE api/amenities/{amenityId}/images/{imageId}
        [HttpDelete("{amenityId:guid}/images/{imageId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(Guid amenityId, Guid imageId)
        {
            await _amenityService.DeleteImageAsync(amenityId, imageId);
            return Ok(ApiResponse<object>.Ok(null!, "Image deleted successfully."));
        }

        // DELETE api/amenities/{amenityId}
        [HttpDelete("{amenityId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAmenity(Guid amenityId)
        {
            await _amenityService.DeleteAmenityAsync(amenityId);
            return Ok(ApiResponse<object>.Ok(null!, "Amenity deleted successfully."));
        }
    }

}
