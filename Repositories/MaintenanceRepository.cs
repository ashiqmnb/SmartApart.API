using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly AppDbContext _db;

        public MaintenanceRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MaintenanceRequest?> GetByIdAsync(Guid requestId)
        {
            return await _db.MaintenanceRequests
                .Include(mr => mr.Resident)
                    .ThenInclude(r => r.User)
                .Include(mr => mr.AssignedStaff)
                .Include(mr => mr.Images)
                .FirstOrDefaultAsync(mr => mr.Id == requestId);
        }

        public async Task<(List<MaintenanceRequest> Requests, int TotalCount)> GetAllAsync(
            Guid? residentId,
            MaintenanceStatus? status,
            MaintenanceCategory? category,
            Priority? priority,
            int page,
            int pageSize)
        {
            var query = _db.MaintenanceRequests
                .Include(mr => mr.Resident)
                    .ThenInclude(r => r.User)
                .Include(mr => mr.AssignedStaff)
                .AsQueryable();

            if (residentId.HasValue)
                query = query.Where(mr => mr.ResidentId == residentId.Value);

            if (status.HasValue)
                query = query.Where(mr => mr.Status == status.Value);

            if (category.HasValue)
                query = query.Where(mr => mr.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(mr => mr.Priority == priority.Value);

            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(mr => mr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (requests, totalCount);
        }

        public async Task AddAsync(MaintenanceRequest request)
        {
            await _db.MaintenanceRequests.AddAsync(request);
        }

        public Task UpdateAsync(MaintenanceRequest request)
        {
            _db.MaintenanceRequests.Update(request);
            return Task.CompletedTask;
        }

        public async Task AddImageAsync(MaintenanceImage image)
        {
            await _db.MaintenanceImages.AddAsync(image);
        }

        public async Task<MaintenanceImage?> GetImageByIdAsync(Guid imageId)
        {
            return await _db.MaintenanceImages
                .FirstOrDefaultAsync(mi => mi.Id == imageId);
        }

        public Task DeleteImageAsync(MaintenanceImage image)
        {
            _db.MaintenanceImages.Remove(image);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}
