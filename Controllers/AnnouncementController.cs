using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Announcements;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ICloudinaryService _cloudinaryService;

        public AnnouncementController(IAnnouncementService announcementService, ICloudinaryService cloudinaryService)
        {
            _announcementService = announcementService;
            _cloudinaryService = cloudinaryService;
        }

        // POST api/announcements
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequestDto dto)
        {
            var callerUserId = GetUserId();
            var result = await _announcementService.CreateAnnouncementAsync(dto, callerUserId);
            return Created($"api/announcements/{result.Id}", ApiResponse<AnnouncementDetailDto>.Ok(result, "Announcement created successfully."));
        }

        // POST api/announcements/{announcementId}/attachments
        [HttpPost("{announcementId:guid}/attachments")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(Guid announcementId, IFormFile file, [FromForm] string fileType)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "announcements");
            var result = await _announcementService.AddAttachmentAsync(announcementId, imageUrl, fileType);
            return Created($"api/announcements/{announcementId}/attachments/{result.Id}", ApiResponse<AnnouncementAttachmentDto>.Ok(result, "Attachment uploaded successfully."));
        }

        // GET api/announcements?noticeType=SocietyNotice&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetPublished([FromQuery] AnnouncementFilterRequestDto filter)
        {
            var result = await _announcementService.GetPublishedAsync(filter);
            return Ok(ApiResponse<PagedResult<AnnouncementListDto>>.Ok(result));
        }

        // GET api/announcements/drafts?page=1&pageSize=10
        [HttpGet("drafts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDrafts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _announcementService.GetDraftsAsync(page, pageSize);
            return Ok(ApiResponse<PagedResult<AnnouncementListDto>>.Ok(result));
        }

        // GET api/announcements/{announcementId}
        [HttpGet("{announcementId:guid}")]
        public async Task<IActionResult> GetById(Guid announcementId)
        {
            var result = await _announcementService.GetByIdAsync(announcementId);
            return Ok(ApiResponse<AnnouncementDetailDto>.Ok(result));
        }

        // PUT api/announcements/{announcementId}
        [HttpPut("{announcementId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAnnouncement(Guid announcementId, [FromBody] UpdateAnnouncementRequestDto dto)
        {
            var result = await _announcementService.UpdateAnnouncementAsync(announcementId, dto);
            return Ok(ApiResponse<AnnouncementDetailDto>.Ok(result, "Announcement updated successfully."));
        }

        // DELETE api/announcements/{announcementId}
        [HttpDelete("{announcementId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAnnouncement(Guid announcementId)
        {
            await _announcementService.DeleteAnnouncementAsync(announcementId);
            return Ok(ApiResponse<object>.Ok(null!, "Announcement deleted successfully."));
        }

        // DELETE api/announcements/{announcementId}/attachments/{attachmentId}
        [HttpDelete("{announcementId:guid}/attachments/{attachmentId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttachment(Guid announcementId, Guid attachmentId)
        {
            await _announcementService.DeleteAttachmentAsync(announcementId, attachmentId);
            return Ok(ApiResponse<object>.Ok(null!, "Attachment deleted successfully."));
        }

        // POST api/announcements/{announcementId}/publish
        [HttpPost("{announcementId:guid}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishNow(Guid announcementId)
        {
            var result = await _announcementService.PublishNowAsync(announcementId);
            return Ok(ApiResponse<AnnouncementDetailDto>.Ok(result, "Announcement published successfully."));
        }

        // ── Helpers ──────────────────────────────────────────────────

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found in token."));
    }

}
