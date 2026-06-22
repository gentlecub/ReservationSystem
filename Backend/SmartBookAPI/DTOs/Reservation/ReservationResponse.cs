namespace SmartBookAPI.DTOs.Reservation;

/// <summary>
/// DTO de respuesta con información de la reserva
/// </summary>
public class ReservationResponse
{
    public int ReservationId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? ResourceLocation { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
