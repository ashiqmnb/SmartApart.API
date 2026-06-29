using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }


        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<User?> GetByPhoneAsync(string phoneNumber)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<(List<User> Users, int TotalCount)> GetAllUsersAsync(string? role, int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (Enum.TryParse<Enums.Role>(role, out var parsedRole))
                    query = query.Where(u => u.Role == parsedRole);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> PhoneExistsForOtherUserAsync(string phoneNumber, Guid excludeUserId)
        {
            return await _db.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber && u.Id != excludeUserId);
        }

        public Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public Task DeleteAsync(User user)
        {
            _db.Users.Remove(user);
            return Task.CompletedTask;
        }
    }
}
