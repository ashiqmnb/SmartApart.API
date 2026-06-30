using SmartApart.API.Common;
using SmartApart.API.DTOs.Complaints;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Services
{
    public interface IComplaintService
    {
        Task<ComplaintDetailDto> CreateComplaintAsync(CreateComplaintRequestDto dto, Guid callerUserId);
        Task<PagedResult<ComplaintListDto>> GetAllComplaintsAsync(ComplaintFilterRequestDto filter, Guid callerUserId, Role callerRole);
        Task<ComplaintDetailDto> GetComplaintByIdAsync(Guid complaintId, Guid callerUserId, Role callerRole);
        Task<ComplaintImageDto> AddImageAsync(Guid complaintId, string imageUrl, Guid callerUserId, Role callerRole);
        Task<ComplaintDetailDto> UpdateStatusAsync(Guid complaintId, UpdateComplaintStatusRequestDto dto);
    }
}
