using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Complaints;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ICloudinaryService _cloudinaryService;

        public ComplaintController(IComplaintService complaintService, ICloudinaryService cloudinaryService)
        {
            _complaintService = complaintService;
            _cloudinaryService = cloudinaryService;
        }

        // POST api/complaints
        [HttpPost]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintRequestDto dto)
        {
            var callerUserId = GetUserId();
            var result = await _complaintService.CreateComplaintAsync(dto, callerUserId);
            return Created($"api/complaints/{result.Id}", ApiResponse<ComplaintDetailDto>.Ok(result, "Complaint submitted successfully."));
        }

        // POST api/complaints/{complaintId}/images
        [HttpPost("{complaintId:guid}/images")]
        [Authorize(Roles = "Resident")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(Guid complaintId, IFormFile file)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();

            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "complaints");
            var result = await _complaintService.AddImageAsync(complaintId, imageUrl, callerUserId, callerRole);

            return Created($"api/complaints/{complaintId}/images/{result.Id}", ApiResponse<ComplaintImageDto>.Ok(result, "Image uploaded successfully."));
        }

        // GET api/complaints?status=Pending&category=Noise&page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetAllComplaints([FromQuery] ComplaintFilterRequestDto filter)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _complaintService.GetAllComplaintsAsync(filter, callerUserId, callerRole);
            return Ok(ApiResponse<PagedResult<ComplaintListDto>>.Ok(result));
        }

        // GET api/complaints/{complaintId}
        [HttpGet("{complaintId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetComplaintById(Guid complaintId)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _complaintService.GetComplaintByIdAsync(complaintId, callerUserId, callerRole);
            return Ok(ApiResponse<ComplaintDetailDto>.Ok(result));
        }

        // PATCH api/complaints/{complaintId}/status
        [HttpPatch("{complaintId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid complaintId, [FromBody] UpdateComplaintStatusRequestDto dto)
        {
            var result = await _complaintService.UpdateStatusAsync(complaintId, dto);
            return Ok(ApiResponse<ComplaintDetailDto>.Ok(result, "Complaint status updated successfully."));
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
