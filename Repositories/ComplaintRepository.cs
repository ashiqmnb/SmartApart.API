using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _db;

        public ComplaintRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Complaint?> GetByIdAsync(Guid complaintId)
        {
            return await _db.Complaints
                .Include(c => c.Resident)
                    .ThenInclude(r => r.User)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == complaintId);
        }

        public async Task<(List<Complaint> Complaints, int TotalCount)> GetAllAsync(
            Guid? residentId,
            ComplaintStatus? status,
            ComplaintCategory? category,
            int page,
            int pageSize)
        {
            var query = _db.Complaints
                .Include(c => c.Resident)
                    .ThenInclude(r => r.User)
                .AsQueryable();

            if (residentId.HasValue)
                query = query.Where(c => c.ResidentId == residentId.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (category.HasValue)
                query = query.Where(c => c.Category == category.Value);

            var totalCount = await query.CountAsync();

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (complaints, totalCount);
        }

        public async Task AddAsync(Complaint complaint)
        {
            await _db.Complaints.AddAsync(complaint);
        }

        public Task UpdateAsync(Complaint complaint)
        {
            _db.Complaints.Update(complaint);
            return Task.CompletedTask;
        }

        public async Task AddImageAsync(ComplaintImage image)
        {
            await _db.ComplaintImages.AddAsync(image);
        }

        public async Task<ComplaintImage?> GetImageByIdAsync(Guid imageId)
        {
            return await _db.ComplaintImages
                .FirstOrDefaultAsync(ci => ci.Id == imageId);
        }

        public Task DeleteImageAsync(ComplaintImage image)
        {
            _db.ComplaintImages.Remove(image);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
