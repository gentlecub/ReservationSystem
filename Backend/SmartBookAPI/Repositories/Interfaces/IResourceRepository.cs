using SmartBookAPI.Models;

namespace SmartBookAPI.Repositories.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(int id);
    Task<IEnumerable<Resource>> GetAllAsync();
    Task<IEnumerable<Resource>> GetActiveAsync();
    Task<Resource> CreateAsync(Resource resource);
    Task<Resource> UpdateAsync(Resource resource);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
