using SmartBookAPI.Models;

namespace SmartBookAPI.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task<Reservation> UpdateAsync(Reservation reservation);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica si existe una reserva que se solape con el horario dado
    /// </summary>
    Task<bool> HasConflictAsync(int resourceId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeReservationId = null);
}
