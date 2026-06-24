using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookAPI.Models;

/// <summary>
/// Representa una reserva de un recurso por un usuario
/// </summary>
public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ResourceId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si se ha enviado un recordatorio para esta reserva
    /// </summary>
    public bool ReminderSent { get; set; } = false;

    // Propiedades de navegación
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("ResourceId")]
    public Resource? Resource { get; set; }
}
