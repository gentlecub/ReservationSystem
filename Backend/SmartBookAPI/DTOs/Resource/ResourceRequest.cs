using System.ComponentModel.DataAnnotations;

namespace SmartBookAPI.DTOs.Resource;

/// <summary>
/// DTO para crear o actualizar un recurso
/// </summary>
public class ResourceRequest
{
    [Required(ErrorMessage = "El nombre del recurso es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
}
