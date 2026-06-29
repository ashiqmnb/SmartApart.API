using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        // User operations
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByPhoneAsync(string phoneNumber);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phoneNumber);
        Task AddUserAsync(User user);

        // Refresh token operations
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string hashedToken);
        void RevokeRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeAllUserRefreshTokensAsync(Guid userId);

        // OTP operations
        Task AddOtpAsync(OtpVerification otp);
        Task<OtpVerification?> GetLatestOtpAsync(string phoneNumber, OtpPurpose purpose);
        void MarkOtpAsUsedAsync(OtpVerification otp);
        Task InvalidatePreviousOtpsAsync(string phoneNumber, OtpPurpose purpose);

        // Persist changes
        Task SaveChangesAsync();
    }
}
