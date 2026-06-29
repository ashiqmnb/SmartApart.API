using SmartApart.API.Entities;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task<(List<User> Users, int TotalCount)> GetAllUsersAsync(string? role, int page, int pageSize);
        Task<bool> PhoneExistsForOtherUserAsync(string phoneNumber, Guid excludeUserId);
        Task UpdateAsync(User user);
        Task AddAsync(User user);
        Task DeleteAsync(User user);
        Task SaveChangesAsync();
    }
}
