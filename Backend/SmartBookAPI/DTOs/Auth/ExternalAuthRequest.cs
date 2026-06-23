using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Auth;

public class ExternalAuthRequest
{
    [Required(ErrorMessage = "El token de acceso es requerido")]
    public string AccessToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "El proveedor es requerido")]
    public string Provider { get; set; } = "Google";
}
