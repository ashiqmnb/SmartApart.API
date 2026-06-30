using SmartApart.API.Common;
using SmartApart.API.DTOs.Visitors;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Services
{
    public interface IVisitorService
    {
        Task<VisitorDetailDto> RegisterVisitorAsync(RegisterVisitorRequestDto dto, Guid securityUserId);
        Task<PagedResult<VisitorListDto>> GetAllVisitorsAsync(VisitorFilterRequestDto filter, Guid callerUserId, Role callerRole);
        Task<VisitorDetailDto> GetVisitorByIdAsync(Guid visitorId, Guid callerUserId, Role callerRole);
        Task<VisitorDetailDto> ApproveVisitorAsync(Guid visitorId, Guid callerUserId);
        Task<VisitorDetailDto> RejectVisitorAsync(Guid visitorId, ApproveRejectVisitorRequestDto dto, Guid callerUserId);
        Task<VisitorDetailDto> RegisterExitAsync(Guid visitorId);
        Task<List<VisitorListDto>> GetPendingApprovalsAsync(Guid callerUserId);
    }
}
