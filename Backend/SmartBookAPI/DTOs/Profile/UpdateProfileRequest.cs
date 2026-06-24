using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Profile;

public class UpdateProfileRequest
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string? FullName { get; set; }

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "La URL de la foto no puede exceder 500 caracteres")]
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string? ProfilePhotoUrl { get; set; }

    // Preferencias de notificación
    public bool? EmailNotifications { get; set; }
    public bool? SmsNotifications { get; set; }
}
