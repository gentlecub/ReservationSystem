using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Data;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;

namespace SmartBookAPI.Repositories.Implementations;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .FirstOrDefaultAsync(r => r.ReservationId == id);
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Cargar las relaciones
        await _context.Entry(reservation).Reference(r => r.User).LoadAsync();
        await _context.Entry(reservation).Reference(r => r.Resource).LoadAsync();
        return reservation;
    }

    public async Task<Reservation> UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return false;

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Reservations.AnyAsync(r => r.ReservationId == id);
    }

    /// <summary>
    /// Verifica si hay conflicto de horarios para el mismo recurso y fecha.
    /// Dos reservas se solapan si:
    /// - Misma fecha y recurso
    /// - El nuevo horario se cruza con uno existente
    /// - La reserva no está cancelada
    /// </summary>
    public async Task<bool> HasConflictAsync(int resourceId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.ResourceId == resourceId)
            .Where(r => r.Date == date)
            .Where(r => r.Status != "Cancelled");

        // Excluir una reserva específica (útil para ediciones)
        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.ReservationId != excludeReservationId.Value);
        }

        // Verificar solapamiento de horarios
        // Conflicto existe si: startTime < existingEnd AND endTime > existingStart
        return await query.AnyAsync(r =>
            startTime < r.EndTime && endTime > r.StartTime);
    }
}
