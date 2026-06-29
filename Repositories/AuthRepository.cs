using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _db;

        public AuthRepository(AppDbContext db)
        {
            _db = db;
        }

        // ── User Operations ──────────────────────────────────────────

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByPhoneAsync(string phoneNumber)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users
                .AnyAsync(u => u.Email == email.ToLower());
        }

        public async Task<bool> PhoneExistsAsync(string phoneNumber)
        {
            return await _db.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task AddUserAsync(User user)
        {
            user.Email = user.Email.ToLower();
            await _db.Users.AddAsync(user);
        }

        // ── Refresh Token Operations ─────────────────────────────────

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _db.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string hashedToken)
        {
            return await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == hashedToken
                                        && !rt.IsRevoked
                                        && rt.ExpiresAt > DateTime.UtcNow);
        }

        public void RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            _db.RefreshTokens.Update(refreshToken);
        }

        public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
        {
            var tokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsRevoked = true;

            _db.RefreshTokens.UpdateRange(tokens);
        }

        // ── OTP Operations ───────────────────────────────────────────

        public async Task AddOtpAsync(OtpVerification otp)
        {
            await _db.OtpVerifications.AddAsync(otp);
        }

        public async Task<OtpVerification?> GetLatestOtpAsync(string phoneNumber, OtpPurpose purpose)
        {
            return await _db.OtpVerifications
                .Where(o => o.PhoneNumber == phoneNumber
                         && o.Purpose == purpose
                         && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public void MarkOtpAsUsedAsync(OtpVerification otp)
        {
            otp.IsUsed = true;
                _db.OtpVerifications.Update(otp);
        }

        public async Task InvalidatePreviousOtpsAsync(string phoneNumber, OtpPurpose purpose)
        {
            var otps = await _db.OtpVerifications
                .Where(o => o.PhoneNumber == phoneNumber
                         && o.Purpose == purpose
                         && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
                otp.IsUsed = true;

            _db.OtpVerifications.UpdateRange(otps);
        }

        // ── Save ─────────────────────────────────────────────────────

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
