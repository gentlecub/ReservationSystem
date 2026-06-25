using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Waitlist;

/// <summary>
/// DTO para crear una entrada en la lista de espera
/// </summary>
public class WaitlistRequest
{
    [Required(ErrorMessage = "El recurso es requerido")]
    public int ResourceId { get; set; }

    [Required(ErrorMessage = "La fecha preferida es requerida")]
    public DateOnly PreferredDate { get; set; }

    public TimeOnly? PreferredStartTime { get; set; }

    public TimeOnly? PreferredEndTime { get; set; }
}
