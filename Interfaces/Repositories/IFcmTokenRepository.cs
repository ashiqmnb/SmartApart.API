using SmartApart.API.Entities;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IFcmTokenRepository
    {
        Task<List<FcmToken>> GetActiveTokensByUserIdAsync(Guid userId);
        Task<FcmToken?> GetByTokenAsync(string token);
        Task AddAsync(FcmToken token);
        Task DeactivateAsync(FcmToken token);
        Task SaveChangesAsync();

    }
}
