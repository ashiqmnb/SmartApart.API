using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly AppDbContext _db;

        public AmenityRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Amenity?> GetByIdAsync(Guid amenityId)
        {
            return await _db.Amenities
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == amenityId);
        }

        public async Task<List<Amenity>> GetAllAsync()
        {
            return await _db.Amenities
                .Include(a => a.Images)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Amenity amenity)
        {
            await _db.Amenities.AddAsync(amenity);
        }

        public Task UpdateAsync(Amenity amenity)
        {
            _db.Amenities.Update(amenity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Amenity amenity)
        {
            _db.Amenities.Remove(amenity);
            return Task.CompletedTask;
        }

        public async Task AddImageAsync(AmenityImage image)
        {
            await _db.AmenityImages.AddAsync(image);
        }

        public async Task<AmenityImage?> GetImageByIdAsync(Guid imageId)
        {
            return await _db.AmenityImages
                .FirstOrDefaultAsync(ai => ai.Id == imageId);
        }

        public Task DeleteImageAsync(AmenityImage image)
        {
            _db.AmenityImages.Remove(image);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}
