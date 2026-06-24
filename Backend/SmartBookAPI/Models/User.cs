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

    [StringLength(255)]
    public string? PasswordHash { get; set; }

    [Required]
    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Nuevos campos para autenticación avanzada
    public bool EmailVerified { get; set; } = false;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool PhoneVerified { get; set; } = false;

    [StringLength(6)]
    public string? PhoneVerificationCode { get; set; }

    public DateTime? PhoneVerificationCodeExpiry { get; set; }

    [StringLength(50)]
    public string AuthProvider { get; set; } = "Local";

    [StringLength(255)]
    public string? ExternalId { get; set; }

    [StringLength(255)]
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    [StringLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? UpdatedAt { get; set; }

    // Preferencias de notificación
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = true;

    // Propiedades de navegación
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
