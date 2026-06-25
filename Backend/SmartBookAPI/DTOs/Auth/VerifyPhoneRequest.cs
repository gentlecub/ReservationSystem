using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Auth;

public class VerifyPhoneRequest
{
    [Required(ErrorMessage = "El número de teléfono es requerido")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de verificación es requerido")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
    public string Code { get; set; } = string.Empty;
}
