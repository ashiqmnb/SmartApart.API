using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IMaintenanceRepository
    {
        Task<MaintenanceRequest?> GetByIdAsync(Guid requestId);
        Task<(List<MaintenanceRequest> Requests, int TotalCount)> GetAllAsync(
            Guid? residentId,
            MaintenanceStatus? status,
            MaintenanceCategory? category,
            Priority? priority,
            int page,
            int pageSize);

        Task AddAsync(MaintenanceRequest request);
        Task UpdateAsync(MaintenanceRequest request);

        Task AddImageAsync(MaintenanceImage image);
        Task<MaintenanceImage?> GetImageByIdAsync(Guid imageId);
        Task DeleteImageAsync(MaintenanceImage image);

        Task SaveChangesAsync();
    }
}
