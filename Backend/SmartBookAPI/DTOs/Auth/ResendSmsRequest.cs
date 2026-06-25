using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Auth;

public class ResendSmsRequest
{
    [Required(ErrorMessage = "El número de teléfono es requerido")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string PhoneNumber { get; set; } = string.Empty;
}
