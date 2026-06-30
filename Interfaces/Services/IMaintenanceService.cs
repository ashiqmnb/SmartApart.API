using SmartApart.API.Common;
using SmartApart.API.DTOs.Maintenance;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Services
{
    public interface IMaintenanceService
    {
        Task<MaintenanceDetailDto> CreateRequestAsync(CreateMaintenanceRequestDto dto, Guid callerUserId);
        Task<PagedResult<MaintenanceListDto>> GetAllRequestsAsync(MaintenanceFilterRequestDto filter, Guid callerUserId, Role callerRole);
        Task<MaintenanceDetailDto> GetRequestByIdAsync(Guid requestId, Guid callerUserId, Role callerRole);
        Task<MaintenanceImageDto> AddImageAsync(Guid requestId, string imageUrl, Guid callerUserId, Role callerRole);
        Task<MaintenanceDetailDto> AssignRequestAsync(Guid requestId, AssignMaintenanceRequestDto dto);
        Task<MaintenanceDetailDto> UpdateStatusAsync(Guid requestId, UpdateMaintenanceStatusRequestDto dto);
        Task CancelRequestAsync(Guid requestId, Guid callerUserId);

    }
}
