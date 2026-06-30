using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class FcmTokenRepository : IFcmTokenRepository
    {
        private readonly AppDbContext _db;

        public FcmTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<FcmToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            return await _db.FcmTokens
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();
        }

        public async Task<FcmToken?> GetByTokenAsync(string token)
        {
            return await _db.FcmTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddAsync(FcmToken token)
        {
            await _db.FcmTokens.AddAsync(token);
        }

        public Task DeactivateAsync(FcmToken token)
        {
            token.IsActive = false;
            _db.FcmTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}
