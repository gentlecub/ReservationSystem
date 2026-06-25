using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Auth;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "El token de verificación es requerido")]
    public string Token { get; set; } = string.Empty;
}
