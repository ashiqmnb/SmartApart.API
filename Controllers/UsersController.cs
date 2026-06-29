using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Users;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/users?role=Resident&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllUsersAsync(role, page, pageSize);
            return Ok(ApiResponse<PagedResult<UserListDto>>.Ok(result));
        }

        // GET api/users/{userId}
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }

        // POST api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequestDto dto)
        {
            var result = await _userService.AdminCreateUserAsync(dto);
            return Created($"api/users/{result.Id}", ApiResponse<UserProfileDto>.Ok(result, "User created successfully."));
        }

        // PATCH api/users/{userId}/toggle-status
        [HttpPatch("{userId:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid userId)
        {
            await _userService.ToggleUserStatusAsync(userId);
            return Ok(ApiResponse<object>.Ok(null!, "User status updated successfully."));
        }

        // DELETE api/users/{userId}
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(ApiResponse<object>.Ok(null!, "User deleted successfully."));
        }
    }
}
