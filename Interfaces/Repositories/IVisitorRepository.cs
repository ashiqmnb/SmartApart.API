using SmartApart.API.DTOs.Visitors;
using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IVisitorRepository
    {
        Task<Visitor?> GetByIdAsync(Guid visitorId);
        Task<(List<Visitor> Visitors, int TotalCount)> GetAllAsync(
            Guid? residentId,
            VisitorStatus? status,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize);
        Task<List<Visitor>> GetPendingByResidentIdAsync(Guid residentId);
        Task AddAsync(Visitor visitor);
        Task UpdateAsync(Visitor visitor);
        Task SaveChangesAsync();

    }
}
