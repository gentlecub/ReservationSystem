using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Reservation;

namespace SmartBookAPI.Services.Interfaces;

public interface IAdminReservationService
{
    /// <summary>
    /// Obtiene reservas con filtros avanzados, paginacion y resumen
    /// </summary>
    Task<ApiResponse<AdminReservationListResponse>> GetWithFiltersAsync(AdminReservationFilterRequest filter);

    /// <summary>
    /// Obtiene reservas activas (Pending o Confirmed, no pasadas)
    /// </summary>
    Task<ApiResponse<IEnumerable<ReservationResponse>>> GetActiveReservationsAsync();

    /// <summary>
    /// Obtiene historial de reservas (pasadas o canceladas)
    /// </summary>
    Task<ApiResponse<IEnumerable<ReservationResponse>>> GetReservationHistoryAsync();

    /// <summary>
    /// Actualiza multiples reservas a la vez
    /// </summary>
    Task<ApiResponse<int>> BulkUpdateStatusAsync(List<int> reservationIds, string newStatus);

    /// <summary>
    /// Obtiene resumen estadistico de reservas
    /// </summary>
    Task<ApiResponse<ReservationSummary>> GetSummaryAsync();
}
