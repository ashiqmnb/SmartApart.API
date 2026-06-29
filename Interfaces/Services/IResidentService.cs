using SmartApart.API.Common;
using SmartApart.API.DTOs.Residents;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Services
{
    public interface IResidentService
    {
        // Resident operations
        Task<PagedResult<object>> GetAllResidentsAsync(int page, int pageSize, Role callerRole);
        Task<object> GetResidentByIdAsync(Guid residentId, Role callerRole);
        Task<ResidentDetailDto> GetMyProfileAsync(Guid userId);
        Task<ResidentDetailDto> UpdateResidentAsync(Guid residentId, UpdateResidentRequestDto dto, Guid callerUserId, Role callerRole);
        Task<List<object>> SearchResidentsAsync(string searchTerm, Role callerRole);


        // Family member operations
        Task<List<FamilyMemberDto>> GetFamilyMembersAsync(Guid residentId, Guid callerUserId, Role callerRole);
        Task<FamilyMemberDto> AddFamilyMemberAsync(Guid residentId, AddFamilyMemberRequestDto dto, Guid callerUserId, Role callerRole);
        Task<FamilyMemberDto> UpdateFamilyMemberAsync(Guid residentId, Guid memberId, UpdateFamilyMemberRequestDto dto, Guid callerUserId, Role callerRole);
        Task DeleteFamilyMemberAsync(Guid residentId, Guid memberId, Guid callerUserId, Role callerRole);
    }
}
