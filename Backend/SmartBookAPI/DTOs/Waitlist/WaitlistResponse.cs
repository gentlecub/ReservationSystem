namespace SmartBookAPI.DTOs.Waitlist;

/// <summary>
/// DTO de respuesta con información de la lista de espera
/// </summary>
public class WaitlistResponse
{
    public int WaitlistId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? ResourceLocation { get; set; }
    public DateOnly PreferredDate { get; set; }
    public TimeOnly? PreferredStartTime { get; set; }
    public TimeOnly? PreferredEndTime { get; set; }
    public int Position { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
}
