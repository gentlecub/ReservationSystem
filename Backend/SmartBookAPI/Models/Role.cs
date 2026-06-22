using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.Models;

/// <summary>
/// Representa un rol de usuario en el sistema (Admin o Client)
/// </summary>
public class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    // Propiedad de navegación: Un rol puede tener muchos usuarios
    public ICollection<User> Users { get; set; } = new List<User>();
}
