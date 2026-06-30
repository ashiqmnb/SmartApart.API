using SmartApart.API.Entities;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IAmenityRepository
    {
        Task<Amenity?> GetByIdAsync(Guid amenityId);
        Task<List<Amenity>> GetAllAsync();

        Task AddAsync(Amenity amenity);
        Task UpdateAsync(Amenity amenity);
        Task DeleteAsync(Amenity amenity);

        Task AddImageAsync(AmenityImage image);
        Task<AmenityImage?> GetImageByIdAsync(Guid imageId);
        Task DeleteImageAsync(AmenityImage image);

        Task SaveChangesAsync();

    }
}
