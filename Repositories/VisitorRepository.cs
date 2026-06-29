using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class VisitorRepository : IVisitorRepository
    {
        private readonly AppDbContext _db;

        public VisitorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Visitor?> GetByIdAsync(Guid visitorId)
        {
            return await _db.Visitors
                .Include(v => v.Resident)
                    .ThenInclude(r => r.User)
                .Include(v => v.Security)
                .Include(v => v.Approver)
                .FirstOrDefaultAsync(v => v.Id == visitorId);
        }

        public async Task<(List<Visitor> Visitors, int TotalCount)> GetAllAsync(
            Guid? residentId,
            VisitorStatus? status,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize)
        {
            var query = _db.Visitors
                .Include(v => v.Resident)
                    .ThenInclude(r => r.User)
                .Include(v => v.Security)
                .AsQueryable();

            if (residentId.HasValue)
                query = query.Where(v => v.ResidentId == residentId.Value);

            if (status.HasValue)
                query = query.Where(v => v.ApprovalStatus == status.Value);

            if (from.HasValue)
                query = query.Where(v => v.EntryTime >= from.Value);

            if (to.HasValue)
                query = query.Where(v => v.EntryTime <= to.Value);

            var totalCount = await query.CountAsync();

            var visitors = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (visitors, totalCount);
        }

        public async Task<List<Visitor>> GetPendingByResidentIdAsync(Guid residentId)
        {
            return await _db.Visitors
                .Include(v => v.Security)
                .Where(v => v.ResidentId == residentId
                         && v.ApprovalStatus == VisitorStatus.Pending)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Visitor visitor)
        {
            await _db.Visitors.AddAsync(visitor);
        }

        public Task UpdateAsync(Visitor visitor)
        {
            _db.Visitors.Update(visitor);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
