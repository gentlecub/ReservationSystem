using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Waitlist;

namespace SmartBookAPI.Services.Interfaces;

public interface IWaitlistService
{
    Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetByUserIdAsync(int userId);
    Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetByResourceIdAsync(int resourceId);
    Task<ApiResponse<WaitlistResponse>> GetByIdAsync(int id);
    Task<ApiResponse<WaitlistResponse>> AddToWaitlistAsync(int userId, WaitlistRequest request);
    Task<ApiResponse> RemoveFromWaitlistAsync(int id, int userId, bool isAdmin);
    Task<ApiResponse> CancelByUserAsync(int id, int userId);

    /// <summary>
    /// Procesa la lista de espera cuando se cancela una reserva
    /// </summary>
    Task<ApiResponse<WaitlistResponse?>> ProcessWaitlistForSlotAsync(int resourceId, DateOnly date, TimeOnly startTime, TimeOnly endTime);

    /// <summary>
    /// Notifica al primer usuario en la cola que hay disponibilidad
    /// </summary>
    Task<ApiResponse> NotifyNextInQueueAsync(int resourceId, DateOnly date);

    /// <summary>
    /// Marca entradas expiradas
    /// </summary>
    Task<ApiResponse<int>> ExpireOldEntriesAsync();

    /// <summary>
    /// Obtiene la posicion en la cola para un usuario
    /// </summary>
    Task<ApiResponse<int>> GetPositionAsync(int waitlistId);
}
