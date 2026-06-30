using SmartApart.API.Common;
using SmartApart.API.DTOs.Announcements;

namespace SmartApart.API.Interfaces.Services
{
    public interface IAnnouncementService
    {
        Task<AnnouncementDetailDto> CreateAnnouncementAsync(CreateAnnouncementRequestDto dto, Guid callerUserId);
        Task<PagedResult<AnnouncementListDto>> GetPublishedAsync(AnnouncementFilterRequestDto filter);
        Task<PagedResult<AnnouncementListDto>> GetDraftsAsync(int page, int pageSize);
        Task<AnnouncementDetailDto> GetByIdAsync(Guid announcementId);
        Task<AnnouncementDetailDto> UpdateAnnouncementAsync(Guid announcementId, UpdateAnnouncementRequestDto dto);
        Task DeleteAnnouncementAsync(Guid announcementId);
        Task<AnnouncementAttachmentDto> AddAttachmentAsync(Guid announcementId, string fileUrl, string fileType);
        Task DeleteAttachmentAsync(Guid announcementId, Guid attachmentId);
        Task<AnnouncementDetailDto> PublishNowAsync(Guid announcementId);

    }
}
