using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Users;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }


        // GET api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var result = await _userService.GetProfileAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }

        // PUT api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
        {
            var userId = GetUserId();
            var result = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile updated successfully."));
        }

        // PATCH api/profile/photo
        [HttpPatch("photo")]
        public async Task<IActionResult> UpdatePhoto([FromForm] IFormFile photo)
        {
            var userId = GetUserId();
            var result = await _userService.UpdateProfilePhotoAsync(userId, photo);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile photo updated successfully."));
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found in token."));
    }
}
