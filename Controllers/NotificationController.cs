using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Notifications;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // POST api/notifications/fcm-token
        [HttpPost("fcm-token")]
        public async Task<IActionResult> RegisterFcmToken([FromBody] RegisterFcmTokenRequestDto dto)
        {
            var userId = GetUserId();
            await _notificationService.RegisterFcmTokenAsync(userId, dto);
            return Ok(ApiResponse<object>.Ok(null!, "Device token registered successfully."));
        }

        // DELETE api/notifications/fcm-token
        [HttpDelete("fcm-token")]
        public async Task<IActionResult> DeregisterFcmToken([FromBody] DeregisterFcmTokenRequestDto dto)
        {
            var userId = GetUserId();
            await _notificationService.DeregisterFcmTokenAsync(userId, dto);
            return Ok(ApiResponse<object>.Ok(null!, "Device token deregistered successfully."));
        }

        // GET api/notifications?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize);
            return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
        }

        // GET api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<UnreadCountDto>.Ok(result));
        }

        // PATCH api/notifications/{notificationId}/read
        [HttpPatch("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(notificationId, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Notification marked as read."));
        }

        // PATCH api/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read."));
        }

        // DELETE api/notifications/{notificationId}
        [HttpDelete("{notificationId:guid}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(notificationId, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Notification deleted successfully."));
        }

        // ── Helper ───────────────────────────────────────────────────

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found in token."));
    }

}
