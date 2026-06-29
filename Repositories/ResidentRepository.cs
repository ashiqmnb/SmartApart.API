using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class ResidentRepository : IResidentRepository
    {
        private readonly AppDbContext _db;

        public ResidentRepository(AppDbContext db)
        {
            _db = db;
        }



        // ── Resident Operations ──────────────────────────────────────
        public async Task<Resident?> GetByIdAsync(Guid residentId)
        {
            return await _db.Residents
                .Include(r => r.User)
                .Include(r => r.FamilyMembers)
                .FirstOrDefaultAsync(r => r.Id == residentId);
        }

        public async Task<Resident?> GetByUserIdAsync(Guid userId)
        {
            return await _db.Residents
                .Include(r => r.User)
                .Include(r => r.FamilyMembers)
                .FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<(List<Resident> Residents, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var query = _db.Residents
                .Include(r => r.User)
                .Where(r => r.User.IsActive)
                .OrderBy(r => r.Block)
                .ThenBy(r => r.ApartmentNumber);

            var totalCount = await query.CountAsync();

            var residents = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (residents, totalCount);
        }

        public async Task<List<Resident>> SearchAsync(string searchTerm)
        {
            return await _db.Residents
                .Include(r => r.User)
                .Where(r => r.User.IsActive &&
                    (EF.Functions.ILike(r.User.FullName, $"%{searchTerm}%") ||
                     EF.Functions.ILike(r.ApartmentNumber, $"%{searchTerm}%") ||
                     EF.Functions.ILike(r.Block, $"%{searchTerm}%")))
                .OrderBy(r => r.Block)
                .ThenBy(r => r.ApartmentNumber)
                .ToListAsync();
        }

        public async Task<bool> ApartmentExistsAsync(string apartmentNumber, string block)
        {
            return await _db.Residents
                .AnyAsync(r => EF.Functions.ILike(r.ApartmentNumber, apartmentNumber)
                            && EF.Functions.ILike(r.Block, block));
        }

        public async Task AddAsync(Resident resident)
        {
            await _db.Residents.AddAsync(resident);
        }

        public Task UpdateAsync(Resident resident)
        {
            _db.Residents.Update(resident);
            return Task.CompletedTask;
        }



        // ── Family Member Operations ─────────────────────────────────
        public async Task<FamilyMember?> GetFamilyMemberByIdAsync(Guid familyMemberId)
        {
            return await _db.FamilyMembers
                .Include(fm => fm.Resident)
                .FirstOrDefaultAsync(fm => fm.Id == familyMemberId);
        }

        public async Task<List<FamilyMember>> GetFamilyMembersByResidentIdAsync(Guid residentId)
        {
            return await _db.FamilyMembers
                .Where(fm => fm.ResidentId == residentId)
                .OrderBy(fm => fm.FullName)
                .ToListAsync();
        }

        public async Task AddFamilyMemberAsync(FamilyMember familyMember)
        {
            await _db.FamilyMembers.AddAsync(familyMember);
        }

        public Task UpdateFamilyMemberAsync(FamilyMember familyMember)
        {
            _db.FamilyMembers.Update(familyMember);
            return Task.CompletedTask;
        }

        public Task DeleteFamilyMemberAsync(FamilyMember familyMember)
        {
            _db.FamilyMembers.Remove(familyMember);
            return Task.CompletedTask;
        }



        // ── Save ─────────────────────────────────────────────────────
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
