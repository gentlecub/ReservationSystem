using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Reservation;

/// <summary>
/// DTO para crear una nueva reserva
/// </summary>
public class ReservationRequest
{
    [Required(ErrorMessage = "El recurso es requerido")]
    public int ResourceId { get; set; }

    [Required(ErrorMessage = "La fecha es requerida")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "La hora de inicio es requerida")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "La hora de fin es requerida")]
    public TimeOnly EndTime { get; set; }
}
