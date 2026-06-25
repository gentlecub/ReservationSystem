namespace SmartBookAPI.DTOs.Reservation;

/// <summary>
/// DTO de respuesta paginada para listado de reservas de admin
/// </summary>
public class AdminReservationListResponse
{
    public List<ReservationResponse> Reservations { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    /// <summary>
    /// Resumen de reservas por estado
    /// </summary>
    public ReservationSummary Summary { get; set; } = new();
}

/// <summary>
/// Resumen estadistico de reservas
/// </summary>
public class ReservationSummary
{
    public int TotalPending { get; set; }
    public int TotalConfirmed { get; set; }
    public int TotalCancelled { get; set; }
    public int TotalActive { get; set; }
    public int TotalHistory { get; set; }
}
