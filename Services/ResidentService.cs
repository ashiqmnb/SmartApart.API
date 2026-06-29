using SmartApart.API.Common;
using SmartApart.API.DTOs.Residents;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class ResidentService : IResidentService
    {
        private readonly IResidentRepository _residentRepo;
        private readonly ILogger<ResidentService> _logger;

        public ResidentService(IResidentRepository residentRepo, ILogger<ResidentService> logger)
        {
            _residentRepo = residentRepo;
            _logger = logger;
        }



        // ── Get All Residents ────────────────────────────────────────
        public async Task<PagedResult<object>> GetAllResidentsAsync(int page, int pageSize, Role callerRole)
        {
            try
            {
                var (residents, totalCount) = await _residentRepo.GetAllAsync(page, pageSize);

                var items = residents.Select(r =>
                    callerRole == Role.Admin
                        ? (object)MapToDetailDto(r)
                        : (object)MapToPublicDto(r))
                    .ToList();

                return new PagedResult<object>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllResidentsAsync failed");
                throw new AppException("Failed to retrieve residents.", 500);
            }
        }



        // ── Get Resident By Id ───────────────────────────────────────
        public async Task<object> GetResidentByIdAsync(Guid residentId, Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                return callerRole == Role.Admin
                    ? (object)MapToDetailDto(resident)
                    : (object)MapToPublicDto(resident);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetResidentByIdAsync failed for {ResidentId}", residentId);
                throw new AppException("Failed to retrieve resident.", 500);
            }
        }



        // ── Get My Profile ───────────────────────────────────────────
        public async Task<ResidentDetailDto> GetMyProfileAsync(Guid userId)
        {
            try
            {
                var resident = await _residentRepo.GetByUserIdAsync(userId)
                    ?? throw new AppException("Resident profile not found.", 404);

                return MapToDetailDto(resident);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyProfileAsync failed for {UserId}", userId);
                throw new AppException("Failed to retrieve profile.", 500);
            }
        }



        // ── Update Resident ──────────────────────────────────────────
        public async Task<ResidentDetailDto> UpdateResidentAsync(
            Guid residentId,
            UpdateResidentRequestDto dto,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                // Residents can only update their own profile
                if (callerRole == Role.Resident && resident.UserId != callerUserId)
                    throw new AppException("You are not authorized to update this profile.", 403);

                if (!Enum.TryParse<OwnershipType>(dto.OwnershipType, out var ownershipType))
                    throw new AppException("Invalid ownership type.", 400);

                resident.ApartmentNumber = dto.ApartmentNumber;
                resident.Block = dto.Block;
                resident.Floor = dto.Floor;
                resident.OwnershipType = ownershipType;
                resident.MoveInDate = dto.MoveInDate;
                resident.MoveOutDate = dto.MoveOutDate;
                resident.EmergencyContactName = dto.EmergencyContactName;
                resident.EmergencyContactPhone = dto.EmergencyContactPhone;

                await _residentRepo.UpdateAsync(resident);
                await _residentRepo.SaveChangesAsync();

                return MapToDetailDto(resident);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateResidentAsync failed for {ResidentId}", residentId);
                throw new AppException("Failed to update resident.", 500);
            }
        }



        // ── Search Residents ─────────────────────────────────────────
        public async Task<List<object>> SearchResidentsAsync(string searchTerm, Role callerRole)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new AppException("Search term is required.", 400);

                var residents = await _residentRepo.SearchAsync(searchTerm);

                return residents.Select(r =>
                    callerRole == Role.Admin
                        ? (object)MapToDetailDto(r)
                        : (object)MapToPublicDto(r))
                    .ToList();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchResidentsAsync failed for term: {Term}", searchTerm);
                throw new AppException("Search failed.", 500);
            }
        }



        // ── Get Family Members ───────────────────────────────────────
        public async Task<List<FamilyMemberDto>> GetFamilyMembersAsync(
            Guid residentId,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                if (callerRole == Role.Resident && resident.UserId != callerUserId)
                    throw new AppException("You are not authorized to view these family members.", 403);

                var members = await _residentRepo.GetFamilyMembersByResidentIdAsync(residentId);
                return members.Select(MapToFamilyMemberDto).ToList();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFamilyMembersAsync failed for {ResidentId}", residentId);
                throw new AppException("Failed to retrieve family members.", 500);
            }
        }



        // ── Add Family Member ────────────────────────────────────────
        public async Task<FamilyMemberDto> AddFamilyMemberAsync(
            Guid residentId,
            AddFamilyMemberRequestDto dto,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                if (callerRole == Role.Resident && resident.UserId != callerUserId)
                    throw new AppException("You are not authorized to add family members for this resident.", 403);

                var familyMember = new FamilyMember
                {
                    Id = Guid.NewGuid(),
                    ResidentId = residentId,
                    FullName = dto.FullName,
                    Relationship = dto.Relationship,
                    PhoneNumber = dto.PhoneNumber,
                    DateOfBirth = dto.DateOfBirth,
                    CreatedAt = DateTime.UtcNow
                };

                await _residentRepo.AddFamilyMemberAsync(familyMember);
                await _residentRepo.SaveChangesAsync();

                return MapToFamilyMemberDto(familyMember);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddFamilyMemberAsync failed for {ResidentId}", residentId);
                throw new AppException("Failed to add family member.", 500);
            }
        }



        // ── Update Family Member ─────────────────────────────────────
        public async Task<FamilyMemberDto> UpdateFamilyMemberAsync(
            Guid residentId,
            Guid memberId,
            UpdateFamilyMemberRequestDto dto,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                if (callerRole == Role.Resident && resident.UserId != callerUserId)
                    throw new AppException("You are not authorized to update family members for this resident.", 403);

                var member = await _residentRepo.GetFamilyMemberByIdAsync(memberId)
                    ?? throw new AppException("Family member not found.", 404);

                if (member.ResidentId != residentId)
                    throw new AppException("Family member does not belong to this resident.", 400);

                member.FullName = dto.FullName;
                member.Relationship = dto.Relationship;
                member.PhoneNumber = dto.PhoneNumber;
                member.DateOfBirth = dto.DateOfBirth;

                await _residentRepo.UpdateFamilyMemberAsync(member);
                await _residentRepo.SaveChangesAsync();

                return MapToFamilyMemberDto(member);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateFamilyMemberAsync failed for member {MemberId}", memberId);
                throw new AppException("Failed to update family member.", 500);
            }
        }



        // ── Delete Family Member ─────────────────────────────────────
        public async Task DeleteFamilyMemberAsync(
            Guid residentId,
            Guid memberId,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(residentId)
                    ?? throw new AppException("Resident not found.", 404);

                if (callerRole == Role.Resident && resident.UserId != callerUserId)
                    throw new AppException("You are not authorized to delete family members for this resident.", 403);

                var member = await _residentRepo.GetFamilyMemberByIdAsync(memberId)
                    ?? throw new AppException("Family member not found.", 404);

                if (member.ResidentId != residentId)
                    throw new AppException("Family member does not belong to this resident.", 400);

                await _residentRepo.DeleteFamilyMemberAsync(member);
                await _residentRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteFamilyMemberAsync failed for member {MemberId}", memberId);
                throw new AppException("Failed to delete family member.", 500);
            }
        }



        // ── Mappers ──────────────────────────────────────────────────
        private static ResidentDetailDto MapToDetailDto(Resident r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            FullName = r.User.FullName,
            Email = r.User.Email,
            PhoneNumber = r.User.PhoneNumber,
            ProfilePhotoUrl = r.User.ProfilePhotoUrl,
            ApartmentNumber = r.ApartmentNumber,
            Block = r.Block,
            Floor = r.Floor,
            OwnershipType = r.OwnershipType.ToString(),
            MoveInDate = r.MoveInDate,
            MoveOutDate = r.MoveOutDate,
            EmergencyContactName = r.EmergencyContactName,
            EmergencyContactPhone = r.EmergencyContactPhone
        };

        private static ResidentPublicDto MapToPublicDto(Resident r) => new()
        {
            Id = r.Id,
            FullName = r.User.FullName,
            Block = r.Block,
            ApartmentNumber = r.ApartmentNumber,
            Floor = r.Floor,
            ProfilePhotoUrl = r.User.ProfilePhotoUrl
        };

        private static FamilyMemberDto MapToFamilyMemberDto(FamilyMember fm) => new()
        {
            Id = fm.Id,
            ResidentId = fm.ResidentId,
            FullName = fm.FullName,
            Relationship = fm.Relationship,
            PhoneNumber = fm.PhoneNumber,
            DateOfBirth = fm.DateOfBirth,
            CreatedAt = fm.CreatedAt
        };
    }
}
