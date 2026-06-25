namespace SmartBookAPI.DTOs.Reservation;

/// <summary>
/// DTO para filtrar reservas en panel de administrador
/// </summary>
public class AdminReservationFilterRequest
{
    /// <summary>
    /// Filtrar por estado: Pending, Confirmed, Cancelled
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filtrar por ID de recurso
    /// </summary>
    public int? ResourceId { get; set; }

    /// <summary>
    /// Filtrar por ID de usuario
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Fecha inicio del rango
    /// </summary>
    public DateOnly? FromDate { get; set; }

    /// <summary>
    /// Fecha fin del rango
    /// </summary>
    public DateOnly? ToDate { get; set; }

    /// <summary>
    /// Solo reservas activas (Pending o Confirmed, no pasadas)
    /// </summary>
    public bool? ActiveOnly { get; set; }

    /// <summary>
    /// Solo historial (reservas pasadas o canceladas)
    /// </summary>
    public bool? HistoryOnly { get; set; }

    /// <summary>
    /// Numero de pagina (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Cantidad de elementos por pagina
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Ordenar por campo: Date, CreatedAt, Status, ResourceName, UserName
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Orden descendente
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
