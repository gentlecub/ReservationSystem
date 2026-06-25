using SmartBookAPI.Models;

namespace SmartBookAPI.Repositories.Interfaces;

public interface IWaitlistRepository
{
    Task<WaitlistEntry?> GetByIdAsync(int id);
    Task<IEnumerable<WaitlistEntry>> GetAllAsync();
    Task<IEnumerable<WaitlistEntry>> GetByUserIdAsync(int userId);
    Task<IEnumerable<WaitlistEntry>> GetByResourceIdAsync(int resourceId);
    Task<IEnumerable<WaitlistEntry>> GetActiveByResourceAndDateAsync(int resourceId, DateOnly date);
    Task<WaitlistEntry> CreateAsync(WaitlistEntry entry);
    Task<WaitlistEntry> UpdateAsync(WaitlistEntry entry);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica si el usuario ya tiene una entrada activa para el mismo recurso y fecha
    /// </summary>
    Task<bool> HasActiveEntryAsync(int userId, int resourceId, DateOnly date);

    /// <summary>
    /// Obtiene el siguiente numero de posicion para una fecha y recurso
    /// </summary>
    Task<int> GetNextPositionAsync(int resourceId, DateOnly date);

    /// <summary>
    /// Obtiene la primera entrada activa en la cola para un recurso y fecha
    /// </summary>
    Task<WaitlistEntry?> GetFirstInQueueAsync(int resourceId, DateOnly date);

    /// <summary>
    /// Obtiene entradas expiradas para limpieza
    /// </summary>
    Task<IEnumerable<WaitlistEntry>> GetExpiredEntriesAsync();

    /// <summary>
    /// Reordena posiciones despues de una eliminacion
    /// </summary>
    Task ReorderPositionsAsync(int resourceId, DateOnly date);
}
