using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBookAPI.Models;

/// <summary>
/// Representa un usuario del sistema de reservas
/// </summary>
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propiedades de navegación
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
