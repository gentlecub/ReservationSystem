namespace SmartBookAPI.DTOs.Notifications;

/// <summary>
/// DTO que contiene los detalles de una reserva para notificaciones
/// </summary>
public class ReservationDetails
{
    public int ReservationId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? ResourceLocation { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
