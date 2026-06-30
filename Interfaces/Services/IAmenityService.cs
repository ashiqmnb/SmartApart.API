using SmartApart.API.DTOs.Amenities;

namespace SmartApart.API.Interfaces.Services
{
    public interface IAmenityService
    {
        Task<AmenityDetailDto> CreateAmenityAsync(CreateAmenityRequestDto dto);
        Task<List<AmenityListDto>> GetAllAsync();
        Task<AmenityDetailDto> GetByIdAsync(Guid amenityId);
        Task<AmenityDetailDto> UpdateAmenityAsync(Guid amenityId, UpdateAmenityRequestDto dto);
        Task<AmenityDetailDto> UpdateAvailabilityAsync(Guid amenityId, UpdateAmenityAvailabilityRequestDto dto);
        Task<AmenityImageDto> AddImageAsync(Guid amenityId, string imageUrl);
        Task DeleteImageAsync(Guid amenityId, Guid imageId);
        Task DeleteAmenityAsync(Guid amenityId);

    }
}
