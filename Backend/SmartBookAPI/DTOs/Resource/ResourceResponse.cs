namespace SmartBookAPI.DTOs.Resource;

/// <summary>
/// DTO de respuesta con información del recurso
/// </summary>
public class ResourceResponse
{
    public int ResourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}
