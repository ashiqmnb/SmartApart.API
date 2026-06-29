using SmartApart.API.Entities;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IResidentRepository
    {
        // Resident operations
        Task<Resident?> GetByIdAsync(Guid residentId);
        Task<Resident?> GetByUserIdAsync(Guid userId);
        Task<(List<Resident> Residents, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<List<Resident>> SearchAsync(string searchTerm);
        Task<bool> ApartmentExistsAsync(string apartmentNumber, string block);
        Task AddAsync(Resident resident);
        Task UpdateAsync(Resident resident);

        // Family member operations
        Task<FamilyMember?> GetFamilyMemberByIdAsync(Guid familyMemberId);
        Task<List<FamilyMember>> GetFamilyMembersByResidentIdAsync(Guid residentId);
        Task AddFamilyMemberAsync(FamilyMember familyMember);
        Task UpdateFamilyMemberAsync(FamilyMember familyMember);
        Task DeleteFamilyMemberAsync(FamilyMember familyMember);

        Task SaveChangesAsync();
    }
}
