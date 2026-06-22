using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Data;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;

namespace SmartBookAPI.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
