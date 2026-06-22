using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Auth;

/// <summary>
/// DTO para registrar un nuevo usuario
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}
