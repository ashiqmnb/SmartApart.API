using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Visitors;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitorService;

        public VisitorController(IVisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // POST api/visitors
        [HttpPost]
        [Authorize(Roles = "Security")]
        public async Task<IActionResult> RegisterVisitor([FromBody] RegisterVisitorRequestDto dto)
        {
            var securityUserId = GetUserId();
            var result = await _visitorService.RegisterVisitorAsync(dto, securityUserId);
            return Created($"api/visitors/{result.Id}", ApiResponse<VisitorDetailDto>.Ok(result, "Visitor registered successfully."));
        }

        // GET api/visitors?status=Pending&from=2024-01-01&to=2024-12-31&page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "Admin,Resident,Security")]
        public async Task<IActionResult> GetAllVisitors([FromQuery] VisitorFilterRequestDto filter)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _visitorService.GetAllVisitorsAsync(filter, callerUserId, callerRole);
            return Ok(ApiResponse<PagedResult<VisitorListDto>>.Ok(result));
        }

        // GET api/visitors/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var callerUserId = GetUserId();
            var result = await _visitorService.GetPendingApprovalsAsync(callerUserId);
            return Ok(ApiResponse<List<VisitorListDto>>.Ok(result));
        }

        // GET api/visitors/{visitorId}
        [HttpGet("{visitorId:guid}")]
        [Authorize(Roles = "Admin,Resident,Security")]
        public async Task<IActionResult> GetVisitorById(Guid visitorId)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _visitorService.GetVisitorByIdAsync(visitorId, callerUserId, callerRole);
            return Ok(ApiResponse<VisitorDetailDto>.Ok(result));
        }

        // PATCH api/visitors/{visitorId}/approve
        [HttpPatch("{visitorId:guid}/approve")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> ApproveVisitor(Guid visitorId)
        {
            var callerUserId = GetUserId();
            var result = await _visitorService.ApproveVisitorAsync(visitorId, callerUserId);
            return Ok(ApiResponse<VisitorDetailDto>.Ok(result, "Visitor approved successfully."));
        }

        // PATCH api/visitors/{visitorId}/reject
        [HttpPatch("{visitorId:guid}/reject")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> RejectVisitor(Guid visitorId, [FromBody] ApproveRejectVisitorRequestDto dto)
        {
            var callerUserId = GetUserId();
            var result = await _visitorService.RejectVisitorAsync(visitorId, dto, callerUserId);
            return Ok(ApiResponse<VisitorDetailDto>.Ok(result, "Visitor rejected successfully."));
        }

        // PATCH api/visitors/{visitorId}/exit
        [HttpPatch("{visitorId:guid}/exit")]
        [Authorize(Roles = "Security")]
        public async Task<IActionResult> RegisterExit(Guid visitorId)
        {
            var result = await _visitorService.RegisterExitAsync(visitorId);
            return Ok(ApiResponse<VisitorDetailDto>.Ok(result, "Visitor exit recorded successfully."));
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
