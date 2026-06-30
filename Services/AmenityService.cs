using SmartApart.API.Common;
using SmartApart.API.DTOs.Amenities;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepo;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<AmenityService> _logger;

        public AmenityService(
            IAmenityRepository amenityRepo,
            ICloudinaryService cloudinaryService,
            ILogger<AmenityService> logger)
        {
            _amenityRepo = amenityRepo;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }



        // ── Create ───────────────────────────────────────────────────

        public async Task<AmenityDetailDto> CreateAmenityAsync(CreateAmenityRequestDto dto)
        {
            try
            {
                var amenity = new Amenity
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Location = dto.Location,
                    OpeningTime = dto.OpeningTime,
                    ClosingTime = dto.ClosingTime,
                    Rules = dto.Rules,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _amenityRepo.AddAsync(amenity);
                await _amenityRepo.SaveChangesAsync();

                var saved = await _amenityRepo.GetByIdAsync(amenity.Id)
                    ?? throw new AppException("Failed to load created amenity.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAmenityAsync failed");
                throw new AppException("Failed to create amenity.", 500);
            }
        }

        // ── Get All ───────────────────────────────────────────────────

        public async Task<List<AmenityListDto>> GetAllAsync()
        {
            try
            {
                var amenities = await _amenityRepo.GetAllAsync();
                return amenities.Select(MapToListDto).ToList();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync failed");
                throw new AppException("Failed to retrieve amenities.", 500);
            }
        }

        // ── Get By Id ─────────────────────────────────────────────────

        public async Task<AmenityDetailDto> GetByIdAsync(Guid amenityId)
        {
            try
            {
                var amenity = await _amenityRepo.GetByIdAsync(amenityId)
                    ?? throw new AppException("Amenity not found.", 404);

                return MapToDetailDto(amenity);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for {Id}", amenityId);
                throw new AppException("Failed to retrieve amenity.", 500);
            }
        }

        // ── Update ────────────────────────────────────────────────────

        public async Task<AmenityDetailDto> UpdateAmenityAsync(Guid amenityId, UpdateAmenityRequestDto dto)
        {
            try
            {
                var amenity = await _amenityRepo.GetByIdAsync(amenityId)
                    ?? throw new AppException("Amenity not found.", 404);

                amenity.Name = dto.Name;
                amenity.Description = dto.Description;
                amenity.Location = dto.Location;
                amenity.OpeningTime = dto.OpeningTime;
                amenity.ClosingTime = dto.ClosingTime;
                amenity.Rules = dto.Rules;
                amenity.UpdatedAt = DateTime.UtcNow;

                await _amenityRepo.UpdateAsync(amenity);
                await _amenityRepo.SaveChangesAsync();

                return MapToDetailDto(amenity);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAmenityAsync failed for {Id}", amenityId);
                throw new AppException("Failed to update amenity.", 500);
            }
        }

        // ── Update Availability ───────────────────────────────────────

        public async Task<AmenityDetailDto> UpdateAvailabilityAsync(Guid amenityId, UpdateAmenityAvailabilityRequestDto dto)
        {
            try
            {
                var amenity = await _amenityRepo.GetByIdAsync(amenityId)
                    ?? throw new AppException("Amenity not found.", 404);

                amenity.Availability = dto.Availability;
                amenity.UpdatedAt = DateTime.UtcNow;

                await _amenityRepo.UpdateAsync(amenity);
                await _amenityRepo.SaveChangesAsync();

                return MapToDetailDto(amenity);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAvailabilityAsync failed for {Id}", amenityId);
                throw new AppException("Failed to update amenity availability.", 500);
            }
        }

        // ── Add Image ─────────────────────────────────────────────────

        public async Task<AmenityImageDto> AddImageAsync(Guid amenityId, string imageUrl)
        {
            try
            {
                var amenity = await _amenityRepo.GetByIdAsync(amenityId)
                    ?? throw new AppException("Amenity not found.", 404);

                var image = new AmenityImage
                {
                    Id = Guid.NewGuid(),
                    AmenityId = amenityId,
                    ImageUrl = imageUrl,
                    UploadedAt = DateTime.UtcNow
                };

                await _amenityRepo.AddImageAsync(image);
                await _amenityRepo.SaveChangesAsync();

                return new AmenityImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    UploadedAt = image.UploadedAt
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddImageAsync failed for amenity {Id}", amenityId);
                throw new AppException("Failed to upload image.", 500);
            }
        }

        // ── Delete Image ──────────────────────────────────────────────

        public async Task DeleteImageAsync(Guid amenityId, Guid imageId)
        {
            try
            {
                var image = await _amenityRepo.GetImageByIdAsync(imageId)
                    ?? throw new AppException("Image not found.", 404);

                if (image.AmenityId != amenityId)
                    throw new AppException("Image does not belong to this amenity.", 400);

                await _cloudinaryService.DeleteImageAsync(image.ImageUrl);

                await _amenityRepo.DeleteImageAsync(image);
                await _amenityRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteImageAsync failed for {ImageId}", imageId);
                throw new AppException("Failed to delete image.", 500);
            }
        }

        // ── Delete Amenity ────────────────────────────────────────────

        public async Task DeleteAmenityAsync(Guid amenityId)
        {
            try
            {
                var amenity = await _amenityRepo.GetByIdAsync(amenityId)
                    ?? throw new AppException("Amenity not found.", 404);

                foreach (var image in amenity.Images)
                {
                    await _cloudinaryService.DeleteImageAsync(image.ImageUrl);
                }

                await _amenityRepo.DeleteAsync(amenity);
                await _amenityRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAmenityAsync failed for {Id}", amenityId);
                throw new AppException("Failed to delete amenity.", 500);
            }
        }

        // ── Mappers ──────────────────────────────────────────────────

        private static AmenityDetailDto MapToDetailDto(Amenity a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Location = a.Location,
            OpeningTime = a.OpeningTime,
            ClosingTime = a.ClosingTime,
            Rules = a.Rules,
            Availability = a.Availability,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Images = a.Images?.Select(i => new AmenityImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                UploadedAt = i.UploadedAt
            }).ToList() ?? new()
        };

        private static AmenityListDto MapToListDto(Amenity a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Location = a.Location,
            Availability = a.Availability,
            ThumbnailUrl = a.Images?.FirstOrDefault()?.ImageUrl
        };
    }

}
