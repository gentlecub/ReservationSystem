using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Reservation;

/// <summary>
/// DTO para actualizar el estado de una reserva (Admin)
/// </summary>
public class ReservationUpdateRequest
{
    [Required(ErrorMessage = "El estado es requerido")]
    [RegularExpression("^(Pending|Confirmed|Cancelled)$",
        ErrorMessage = "El estado debe ser: Pending, Confirmed o Cancelled")]
    public string Status { get; set; } = string.Empty;
}
