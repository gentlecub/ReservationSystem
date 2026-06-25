using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Data;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;

namespace SmartBookAPI.Repositories.Implementations;

public class WaitlistRepository : IWaitlistRepository
{
    private readonly AppDbContext _context;

    public WaitlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WaitlistEntry?> GetByIdAsync(int id)
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .FirstOrDefaultAsync(w => w.WaitlistId == id);
    }

    public async Task<IEnumerable<WaitlistEntry>> GetAllAsync()
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .OrderBy(w => w.PreferredDate)
            .ThenBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<WaitlistEntry>> GetByUserIdAsync(int userId)
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.PreferredDate)
            .ThenBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<WaitlistEntry>> GetByResourceIdAsync(int resourceId)
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .Where(w => w.ResourceId == resourceId)
            .Where(w => w.Status == "Active" || w.Status == "Notified")
            .OrderBy(w => w.PreferredDate)
            .ThenBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<WaitlistEntry>> GetActiveByResourceAndDateAsync(int resourceId, DateOnly date)
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .Where(w => w.ResourceId == resourceId)
            .Where(w => w.PreferredDate == date)
            .Where(w => w.Status == "Active")
            .OrderBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<WaitlistEntry> CreateAsync(WaitlistEntry entry)
    {
        _context.WaitlistEntries.Add(entry);
        await _context.SaveChangesAsync();

        await _context.Entry(entry).Reference(w => w.User).LoadAsync();
        await _context.Entry(entry).Reference(w => w.Resource).LoadAsync();
        return entry;
    }

    public async Task<WaitlistEntry> UpdateAsync(WaitlistEntry entry)
    {
        _context.WaitlistEntries.Update(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.WaitlistEntries.FindAsync(id);
        if (entry == null) return false;

        _context.WaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.WaitlistEntries.AnyAsync(w => w.WaitlistId == id);
    }

    public async Task<bool> HasActiveEntryAsync(int userId, int resourceId, DateOnly date)
    {
        return await _context.WaitlistEntries
            .AnyAsync(w => w.UserId == userId
                        && w.ResourceId == resourceId
                        && w.PreferredDate == date
                        && w.Status == "Active");
    }

    public async Task<int> GetNextPositionAsync(int resourceId, DateOnly date)
    {
        var maxPosition = await _context.WaitlistEntries
            .Where(w => w.ResourceId == resourceId)
            .Where(w => w.PreferredDate == date)
            .Where(w => w.Status == "Active")
            .MaxAsync(w => (int?)w.Position) ?? 0;

        return maxPosition + 1;
    }

    public async Task<WaitlistEntry?> GetFirstInQueueAsync(int resourceId, DateOnly date)
    {
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .Where(w => w.ResourceId == resourceId)
            .Where(w => w.PreferredDate == date)
            .Where(w => w.Status == "Active")
            .OrderBy(w => w.Position)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WaitlistEntry>> GetExpiredEntriesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Resource)
            .Where(w => w.Status == "Active" || w.Status == "Notified")
            .Where(w => w.ExpiresAt != null && w.ExpiresAt < now)
            .ToListAsync();
    }

    public async Task ReorderPositionsAsync(int resourceId, DateOnly date)
    {
        var entries = await _context.WaitlistEntries
            .Where(w => w.ResourceId == resourceId)
            .Where(w => w.PreferredDate == date)
            .Where(w => w.Status == "Active")
            .OrderBy(w => w.Position)
            .ToListAsync();

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Position = i + 1;
        }

        await _context.SaveChangesAsync();
    }
}
