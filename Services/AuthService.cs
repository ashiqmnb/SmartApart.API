using SmartApart.API.Common;
using SmartApart.API.DTOs.Auth;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IJwtService jwtService, ILogger<AuthService> logger, IConfiguration config)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
            _logger = logger;
            _config = config;
        }


        // ── Register ─────────────────────────────────────────────────
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                if (await _authRepo.EmailExistsAsync(dto.Email))
                    throw new AppException("Email is already registered.", 409);

                if (await _authRepo.PhoneExistsAsync(dto.PhoneNumber))
                    throw new AppException("Phone number is already registered.", 409);

                if (!Enum.TryParse<Role>(dto.Role, out var role))
                    throw new AppException("Invalid role specified.", 400);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Email = dto.Email.ToLower(),
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _authRepo.AddUserAsync(user);

                // Generate and store OTP for phone verification
                await StoreOtpAsync(dto.PhoneNumber, OtpPurpose.Registration);

                await _authRepo.SaveChangesAsync();

                // TODO: Send OTP via SMS provider in a later phase
                // For now OTP is logged for testing
                _logger.LogInformation("OTP generated for {Phone} — check DB for value", dto.PhoneNumber);

                return new AuthResponseDto
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync failed for {Email}", dto.Email);
                throw new AppException("Registration failed. Please try again.", 500);
            }
        }



        // ── Verify OTP ───────────────────────────────────────────────
        public async Task VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            try
            {
                if (!Enum.TryParse<OtpPurpose>(dto.Purpose, out var purpose))
                    throw new AppException("Invalid OTP purpose.", 400);

                var hashedCode = _jwtService.HashToken(dto.OtpCode);

                var otp = await _authRepo.GetLatestOtpAsync(dto.PhoneNumber, purpose);

                if (otp == null)
                    throw new AppException("OTP not found or has expired.", 400);

                if (otp.OtpCode != hashedCode)
                    throw new AppException("Invalid OTP code.", 400);

                _authRepo.MarkOtpAsUsedAsync(otp);
                await _authRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtpAsync failed for {Phone}", dto.PhoneNumber);
                throw new AppException("OTP verification failed. Please try again.", 500);
            }
        }



        // ── Login ────────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                var user = await _authRepo.GetUserByEmailAsync(dto.Email)
                    ?? throw new AppException("Invalid email or password.", 401);

                if (!user.IsActive)
                    throw new AppException("Your account has been deactivated.", 403);

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    throw new AppException("Invalid email or password.", 401);

                var accessToken = _jwtService.GenerateAccessToken(user);
                var rawRefreshToken = _jwtService.GenerateRefreshToken();

                var refreshTokenExpiryDays = int.Parse(
                    _config["JwtSettings:RefreshTokenExpiryDays"] ?? "30");

                await _authRepo.AddRefreshTokenAsync(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = _jwtService.HashToken(rawRefreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepo.SaveChangesAsync();

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = rawRefreshToken,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed for {Email}", dto.Email);
                throw new AppException("Login failed. Please try again.", 500);
            }
        }
        


        // ── Refresh Token ────────────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            try
            {
                var hashedToken = _jwtService.HashToken(dto.RefreshToken);

                var storedToken = await _authRepo.GetRefreshTokenAsync(hashedToken)
                    ?? throw new AppException("Invalid or expired refresh token.", 401);

                // Rotate — revoke old, issue new
                _authRepo.RevokeRefreshTokenAsync(storedToken);

                var newRawRefreshToken = _jwtService.GenerateRefreshToken();
                var refreshTokenExpiryDays = int.Parse(
                    _config["JwtSettings:RefreshTokenExpiryDays"] ?? "30");

                await _authRepo.AddRefreshTokenAsync(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = storedToken.UserId,
                    Token = _jwtService.HashToken(newRawRefreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepo.SaveChangesAsync();

                var newAccessToken = _jwtService.GenerateAccessToken(storedToken.User);

                return new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRawRefreshToken,
                    FullName = storedToken.User.FullName,
                    Email = storedToken.User.Email,
                    Role = storedToken.User.Role.ToString()
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenAsync failed");
                throw new AppException("Token refresh failed. Please log in again.", 500);
            }
        }
        


        // ── Logout ───────────────────────────────────────────────────
        public async Task LogoutAsync(string refreshToken)
        {
            try
            {
                var hashedToken = _jwtService.HashToken(refreshToken);
                var storedToken = await _authRepo.GetRefreshTokenAsync(hashedToken);

                if (storedToken != null)
                {
                    _authRepo.RevokeRefreshTokenAsync(storedToken);
                    await _authRepo.SaveChangesAsync();
                }
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogoutAsync failed");
                throw new AppException("Logout failed. Please try again.", 500);
            }
        }



        // ── Forgot Password ──────────────────────────────────────────
        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            try
            {
                var user = await _authRepo.GetUserByPhoneAsync(dto.PhoneNumber)
                    ?? throw new AppException("No account found with this phone number.", 404);

                if (!user.IsActive)
                    throw new AppException("Your account has been deactivated.", 403);

                await StoreOtpAsync(dto.PhoneNumber, OtpPurpose.ForgotPassword);
                await _authRepo.SaveChangesAsync();

                // TODO: Send OTP via SMS provider
                _logger.LogInformation("Password reset OTP generated for {Phone}", dto.PhoneNumber);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPasswordAsync failed for {Phone}", dto.PhoneNumber);
                throw new AppException("Request failed. Please try again.", 500);
            }
        }



        // ── Reset Password ───────────────────────────────────────────
        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            try
            {
                var hashedCode = _jwtService.HashToken(dto.OtpCode);

                var otp = await _authRepo.GetLatestOtpAsync(dto.PhoneNumber, OtpPurpose.ForgotPassword)
                    ?? throw new AppException("OTP not found or has expired.", 400);

                if (otp.OtpCode != hashedCode)
                    throw new AppException("Invalid OTP code.", 400);

                var user = await _authRepo.GetUserByPhoneAsync(dto.PhoneNumber)
                    ?? throw new AppException("User not found.", 404);

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _authRepo.MarkOtpAsUsedAsync(otp);

                // Revoke all existing refresh tokens on password reset
                await _authRepo.RevokeAllUserRefreshTokensAsync(user.Id);

                await _authRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPasswordAsync failed for {Phone}", dto.PhoneNumber);
                throw new AppException("Password reset failed. Please try again.", 500);
            }
        }



        // ── Change Password ──────────────────────────────────────────
        public async Task ChangePasswordAsync(ChangePasswordRequestDto dto, Guid userId)
        {
            try
            {
                var user = await _authRepo.GetUserByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                    throw new AppException("Current password is incorrect.", 400);

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _authRepo.RevokeAllUserRefreshTokensAsync(user.Id);
                await _authRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePasswordAsync failed for userId {UserId}", userId);
                throw new AppException("Password change failed. Please try again.", 500);
            }
        }



        // ── Resend OTP ───────────────────────────────────────────────
        public async Task ResendOtpAsync(ResendOtpRequestDto dto)
        {
            try
            {
                if (!Enum.TryParse<OtpPurpose>(dto.Purpose, out var purpose))
                    throw new AppException("Invalid OTP purpose.", 400);

                var user = await _authRepo.GetUserByPhoneAsync(dto.PhoneNumber)
                    ?? throw new AppException("No account found with this phone number.", 404);

                // Invalidate all previous unused OTPs for this purpose
                await _authRepo.InvalidatePreviousOtpsAsync(dto.PhoneNumber, purpose);

                await StoreOtpAsync(dto.PhoneNumber, purpose);
                await _authRepo.SaveChangesAsync();

                _logger.LogInformation("OTP resent for {Phone} — purpose: {Purpose}", dto.PhoneNumber, purpose);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResendOtpAsync failed for {Phone}", dto.PhoneNumber);
                throw new AppException("Failed to resend OTP. Please try again.", 500);
            }
        }



        // ── Private Helper ───────────────────────────────────────────
        private async Task StoreOtpAsync(string phoneNumber, OtpPurpose purpose)
        {
            // Invalidate previous OTPs first
            await _authRepo.InvalidatePreviousOtpsAsync(phoneNumber, purpose);

            // Generate 6-digit OTP
            var rawOtp = Random.Shared.Next(100000, 999999).ToString();
            var hashedOtp = _jwtService.HashToken(rawOtp);

            _logger.LogInformation("OTP for {Phone}: {Otp}", phoneNumber, rawOtp);

            await _authRepo.AddOtpAsync(new OtpVerification
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phoneNumber,
                OtpCode = hashedOtp,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });
        }



    }
}
