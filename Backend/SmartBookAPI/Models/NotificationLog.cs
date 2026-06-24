using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookAPI.Models;

/// <summary>
/// Registro de notificaciones enviadas
/// </summary>
public class NotificationLog
{
    [Key]
    public int Id { get; set; }

    public int? ReservationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // Created, Confirmed, Cancelled, Reminder, Modified

    [Required]
    [StringLength(20)]
    public string Channel { get; set; } = string.Empty; // Email, SMS

    public bool Success { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Propiedades de navegación
    [ForeignKey("ReservationId")]
    public Reservation? Reservation { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
