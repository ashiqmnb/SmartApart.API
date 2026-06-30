using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IComplaintRepository
    {
        Task<Complaint?> GetByIdAsync(Guid complaintId);
        Task<(List<Complaint> Complaints, int TotalCount)> GetAllAsync(
            Guid? residentId,
            ComplaintStatus? status,
            ComplaintCategory? category,
            int page,
            int pageSize);

        Task AddAsync(Complaint complaint);
        Task UpdateAsync(Complaint complaint);

        Task AddImageAsync(ComplaintImage image);
        Task<ComplaintImage?> GetImageByIdAsync(Guid imageId);
        Task DeleteImageAsync(ComplaintImage image);

        Task SaveChangesAsync();

    }
}
