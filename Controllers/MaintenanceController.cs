using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Maintenance;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly ICloudinaryService _cloudinaryService;

        public MaintenanceController(IMaintenanceService maintenanceService, ICloudinaryService cloudinaryService)
        {
            _maintenanceService = maintenanceService;
            _cloudinaryService = cloudinaryService;
        }

        // POST api/maintenance
        [HttpPost]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaintenanceRequestDto dto)
        {
            var callerUserId = GetUserId();
            var result = await _maintenanceService.CreateRequestAsync(dto, callerUserId);
            return Created($"api/maintenance/{result.Id}", ApiResponse<MaintenanceDetailDto>.Ok(result, "Maintenance request created successfully."));
        }

        // POST api/maintenance/{requestId}/images
        [HttpPost("{requestId:guid}/images")]
        [Authorize(Roles = "Resident")]
        [Consumes("multipart/form-data")]

        public async Task<IActionResult> UploadImage(Guid requestId, IFormFile file)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();

            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "maintenance");
            var result = await _maintenanceService.AddImageAsync(requestId, imageUrl, callerUserId, callerRole);

            return Created($"api/maintenance/{requestId}/images/{result.Id}", ApiResponse<MaintenanceImageDto>.Ok(result, "Image uploaded successfully."));
        }

        // GET api/maintenance?status=Open&category=Plumbing&priority=High&page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetAllRequests([FromQuery] MaintenanceFilterRequestDto filter)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _maintenanceService.GetAllRequestsAsync(filter, callerUserId, callerRole);
            return Ok(ApiResponse<PagedResult<MaintenanceListDto>>.Ok(result));
        }

        // GET api/maintenance/{requestId}
        [HttpGet("{requestId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetRequestById(Guid requestId)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _maintenanceService.GetRequestByIdAsync(requestId, callerUserId, callerRole);
            return Ok(ApiResponse<MaintenanceDetailDto>.Ok(result));
        }

        // PATCH api/maintenance/{requestId}/assign
        [HttpPatch("{requestId:guid}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRequest(Guid requestId, [FromBody] AssignMaintenanceRequestDto dto)
        {
            var result = await _maintenanceService.AssignRequestAsync(requestId, dto);
            return Ok(ApiResponse<MaintenanceDetailDto>.Ok(result, "Request assigned successfully."));
        }

        // PATCH api/maintenance/{requestId}/status
        [HttpPatch("{requestId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid requestId, [FromBody] UpdateMaintenanceStatusRequestDto dto)
        {
            var result = await _maintenanceService.UpdateStatusAsync(requestId, dto);
            return Ok(ApiResponse<MaintenanceDetailDto>.Ok(result, "Request status updated successfully."));
        }

        // DELETE api/maintenance/{requestId}
        [HttpDelete("{requestId:guid}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CancelRequest(Guid requestId)
        {
            var callerUserId = GetUserId();
            await _maintenanceService.CancelRequestAsync(requestId, callerUserId);
            return Ok(ApiResponse<object>.Ok(null!, "Maintenance request cancelled successfully."));
        }

        // ── Helpers ──────────────────────────────────────────────────

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found in token."));

        private Role GetCallerRole()
        {
            var roleStr = User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Role not found in token.");
            return Enum.Parse<Role>(roleStr);
        }
    }

}
