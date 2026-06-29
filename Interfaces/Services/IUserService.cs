using SmartApart.API.Common;
using SmartApart.API.DTOs.Users;

namespace SmartApart.API.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto);
        Task<UserProfileDto> UpdateProfilePhotoAsync(Guid userId, IFormFile photo);
        Task<PagedResult<UserListDto>> GetAllUsersAsync(string? role, int page, int pageSize);
        Task<UserProfileDto> GetUserByIdAsync(Guid userId);
        Task<UserProfileDto> AdminCreateUserAsync(AdminCreateUserRequestDto dto);
        Task ToggleUserStatusAsync(Guid userId);
        Task DeleteUserAsync(Guid userId);
    }
}
