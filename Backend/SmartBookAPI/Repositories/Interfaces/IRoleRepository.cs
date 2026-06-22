using SmartBookAPI.Models;

namespace SmartBookAPI.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string roleName);
    Task<IEnumerable<Role>> GetAllAsync();
}
