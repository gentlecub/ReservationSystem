using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.Models;

/// <summary>
/// Representa un recurso que puede ser reservado (cancha, sala, etc.)
/// </summary>
public class Resource
{
    [Key]
    public int ResourceId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    // Propiedad de navegación: Un recurso puede tener muchas reservas
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
