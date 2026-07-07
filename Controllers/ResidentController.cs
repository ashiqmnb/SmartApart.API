using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Residents;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResidentController : ControllerBase
    {
        private readonly IResidentService _residentService;

        public ResidentController(IResidentService residentService)
        {
            _residentService = residentService;
        }

        // GET api/residents?page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetAllResidents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var callerRole = GetCallerRole();
            var result = await _residentService.GetAllResidentsAsync(page, pageSize, callerRole);
            return Ok(ApiResponse<PagedResult<object>>.Ok(result));
        }

        // GET api/residents/search?q=John
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Resident,Security")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var callerRole = GetCallerRole();
            var result = await _residentService.SearchResidentsAsync(q, callerRole);
            return Ok(ApiResponse<List<object>>.Ok(result));
        }

        // GET api/residents/my-profile
        [HttpGet("my-profile")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            var result = await _residentService.GetMyProfileAsync(userId);
            return Ok(ApiResponse<ResidentDetailDto>.Ok(result));
        }

        // GET api/residents/{residentId}
        [HttpGet("{residentId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetResidentById(Guid residentId)
        {
            var callerRole = GetCallerRole();
            var result = await _residentService.GetResidentByIdAsync(residentId, callerRole);
            return Ok(ApiResponse<object>.Ok(result));
        }

        // PUT api/residents/{residentId}
        [HttpPut("{residentId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> UpdateResident(
            Guid residentId,
            [FromBody] UpdateResidentRequestDto dto)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _residentService.UpdateResidentAsync(residentId, dto, callerUserId, callerRole);
            return Ok(ApiResponse<ResidentDetailDto>.Ok(result, "Resident updated successfully."));
        }

        // GET api/residents/{residentId}/family-members
        [HttpGet("{residentId:guid}/family-members")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> GetFamilyMembers(Guid residentId)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _residentService.GetFamilyMembersAsync(residentId, callerUserId, callerRole);
            return Ok(ApiResponse<List<FamilyMemberDto>>.Ok(result));
        }

        // POST api/residents/{residentId}/family-members
        [HttpPost("{residentId:guid}/family-members")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> AddFamilyMember(
            Guid residentId,
            [FromBody] AddFamilyMemberRequestDto dto)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _residentService.AddFamilyMemberAsync(residentId, dto, callerUserId, callerRole);
            return Created(
                $"api/residents/{residentId}/family-members/{result.Id}",
                ApiResponse<FamilyMemberDto>.Ok(result, "Family member added successfully."));
        }

        // PUT api/residents/{residentId}/family-members/{memberId}
        [HttpPut("{residentId:guid}/family-members/{memberId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> UpdateFamilyMember(
            Guid residentId,
            Guid memberId,
            [FromBody] UpdateFamilyMemberRequestDto dto)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            var result = await _residentService.UpdateFamilyMemberAsync(residentId, memberId, dto, callerUserId, callerRole);
            return Ok(ApiResponse<FamilyMemberDto>.Ok(result, "Family member updated successfully."));
        }

        // DELETE api/residents/{residentId}/family-members/{memberId}
        [HttpDelete("{residentId:guid}/family-members/{memberId:guid}")]
        [Authorize(Roles = "Admin,Resident")]
        public async Task<IActionResult> DeleteFamilyMember(Guid residentId, Guid memberId)
        {
            var callerUserId = GetUserId();
            var callerRole = GetCallerRole();
            await _residentService.DeleteFamilyMemberAsync(residentId, memberId, callerUserId, callerRole);
            return Ok(ApiResponse<object>.Ok(null!, "Family member deleted successfully."));
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
