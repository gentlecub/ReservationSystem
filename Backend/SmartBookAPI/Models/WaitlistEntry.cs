using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookAPI.Models;

/// <summary>
/// Representa una entrada en la lista de espera para un recurso
/// </summary>
public class WaitlistEntry
{
    [Key]
    public int WaitlistId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ResourceId { get; set; }

    [Required]
    public DateOnly PreferredDate { get; set; }

    public TimeOnly? PreferredStartTime { get; set; }

    public TimeOnly? PreferredEndTime { get; set; }

    /// <summary>
    /// Posicion en la cola de espera (1 = primero)
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Estado: Active, Notified, Fulfilled, Cancelled, Expired
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de expiracion de la entrada en lista de espera
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Fecha en que se notifico al usuario de disponibilidad
    /// </summary>
    public DateTime? NotifiedAt { get; set; }

    // Propiedades de navegacion
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("ResourceId")]
    public Resource? Resource { get; set; }
}
