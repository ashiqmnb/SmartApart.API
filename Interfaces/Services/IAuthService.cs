using SmartApart.API.DTOs.Auth;

namespace SmartApart.API.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task LogoutAsync(string refreshToken);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task ChangePasswordAsync(ChangePasswordRequestDto dto, Guid userId);
        Task ResendOtpAsync(ResendOtpRequestDto dto);
    }
}
