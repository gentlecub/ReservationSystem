using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Data;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;

namespace SmartBookAPI.Repositories.Implementations;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _context;

    public ResourceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources.FindAsync(id);
    }

    public async Task<IEnumerable<Resource>> GetAllAsync()
    {
        return await _context.Resources
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetActiveAsync()
    {
        return await _context.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Resource> CreateAsync(Resource resource)
    {
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task<Resource> UpdateAsync(Resource resource)
    {
        _context.Resources.Update(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return false;

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Resources.AnyAsync(r => r.ResourceId == id);
    }
}
