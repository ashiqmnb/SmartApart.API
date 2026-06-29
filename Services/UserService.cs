using SmartApart.API.Common;
using SmartApart.API.DTOs.Users;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }



        // ── Get Profile ──────────────────────────────────────────────
        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                return MapToProfileDto(user);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfileAsync failed for {UserId}", userId);
                throw new AppException("Failed to retrieve profile.", 500);
            }
        }



        // ── Update Profile ───────────────────────────────────────────
        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                if (dto.PhoneNumber != user.PhoneNumber)
                {
                    var phoneInUse = await _userRepo.PhoneExistsForOtherUserAsync(dto.PhoneNumber, userId);
                    if (phoneInUse)
                        throw new AppException("Phone number is already in use.", 409);
                }

                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;

                await _userRepo.UpdateAsync(user);
                await _userRepo.SaveChangesAsync();

                return MapToProfileDto(user);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfileAsync failed for {UserId}", userId);
                throw new AppException("Failed to update profile.", 500);
            }
        }



        // ── Update Profile Photo ─────────────────────────────────────
        public async Task<UserProfileDto> UpdateProfilePhotoAsync(Guid userId, IFormFile photo)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                // Cloudinary upload will be wired in Phase 1.6
                // For now store a placeholder
                user.ProfilePhotoUrl = $"pending-upload/{photo.FileName}";

                await _userRepo.UpdateAsync(user);
                await _userRepo.SaveChangesAsync();

                return MapToProfileDto(user);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfilePhotoAsync failed for {UserId}", userId);
                throw new AppException("Failed to update profile photo.", 500);
            }
        }



        // ── Admin: Get All Users ─────────────────────────────────────
        public async Task<PagedResult<UserListDto>> GetAllUsersAsync(string? role, int page, int pageSize)
        {
            try
            {
                var (users, totalCount) = await _userRepo.GetAllUsersAsync(role, page, pageSize);

                return new PagedResult<UserListDto>
                {
                    Items = users.Select(MapToListDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllUsersAsync failed");
                throw new AppException("Failed to retrieve users.", 500);
            }
        }



        // ── Admin: Get User By Id ────────────────────────────────────
        public async Task<UserProfileDto> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                return MapToProfileDto(user);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserByIdAsync failed for {UserId}", userId);
                throw new AppException("Failed to retrieve user.", 500);
            }
        }



        // ── Admin: Create User ───────────────────────────────────────
        public async Task<UserProfileDto> AdminCreateUserAsync(AdminCreateUserRequestDto dto)
        {
            try
            {
                var emailExists = await _userRepo.GetByEmailAsync(dto.Email);
                if (emailExists != null)
                    throw new AppException("Email is already registered.", 409);

                var phoneExists = await _userRepo.GetByPhoneAsync(dto.PhoneNumber);
                if (phoneExists != null)
                    throw new AppException("Phone number is already registered.", 409);

                if (!Enum.TryParse<Role>(dto.Role, out var role))
                    throw new AppException("Invalid role specified.", 400);

                // Generate a temporary password
                var tempPassword = $"SmartApart@{Random.Shared.Next(1000, 9999)}";

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Email = dto.Email.ToLower(),
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                // TODO: Send temp password via SMS in Phase 1.7
                _logger.LogInformation("Admin created user {Email} with temp password: {TempPassword}",
                    user.Email, tempPassword);

                return MapToProfileDto(user);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminCreateUserAsync failed for {Email}", dto.Email);
                throw new AppException("Failed to create user.", 500);
            }
        }



        // ── Admin: Toggle User Status ────────────────────────────────
        public async Task ToggleUserStatusAsync(Guid userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                user.IsActive = !user.IsActive;

                await _userRepo.UpdateAsync(user);
                await _userRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleUserStatusAsync failed for {UserId}", userId);
                throw new AppException("Failed to update user status.", 500);
            }
        }



        // ── Admin: Delete User ───────────────────────────────────────
        public async Task DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId)
                    ?? throw new AppException("User not found.", 404);

                if (user.Role == Role.Resident)
                    throw new AppException("Resident users cannot be hard deleted. Use toggle status instead.", 400);

                await _userRepo.DeleteAsync(user);
                await _userRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUserAsync failed for {UserId}", userId);
                throw new AppException("Failed to delete user.", 500);
            }
        }



        // ── Mappers ──────────────────────────────────────────────────
        private static UserProfileDto MapToProfileDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        private static UserListDto MapToListDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
