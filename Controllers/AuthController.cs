using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartApart.API.Common;
using SmartApart.API.DTOs.Auth;
using SmartApart.API.Interfaces.Services;
using System.Security.Claims;

namespace SmartApart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful. Please verify your phone number with the OTP sent."));
        }

        // POST api/auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            await _authService.VerifyOtpAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, "OTP verified successfully."));
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
        }

        // POST api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed successfully."));
        }

        // POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken);
            return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
        }

        // POST api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, "OTP sent to your registered phone number."));
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, "Password reset successfully."));
        }

        // POST api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found in token."));

            await _authService.ChangePasswordAsync(dto, userId);
            return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
        }

        // POST api/auth/resend-otp
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
        {
            await _authService.ResendOtpAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, "OTP resent successfully."));
        }
    }
}
