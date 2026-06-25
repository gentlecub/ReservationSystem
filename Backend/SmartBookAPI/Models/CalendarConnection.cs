namespace SmartBookAPI.Models;

/// <summary>
/// Representa una conexion del usuario a un calendario externo (Google Calendar o Microsoft Outlook)
/// </summary>
public class CalendarConnection
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>
    /// Proveedor del calendario: "Google" o "Microsoft"
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Token de acceso para la API del calendario (encriptado)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token de refresco para renovar el acceso (encriptado)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expiracion del token de acceso
    /// </summary>
    public DateTime TokenExpiry { get; set; }

    /// <summary>
    /// ID del calendario principal del usuario
    /// </summary>
    public string? CalendarId { get; set; }

    /// <summary>
    /// Email asociado a la cuenta del calendario
    /// </summary>
    public string? CalendarEmail { get; set; }

    /// <summary>
    /// Fecha de conexion
    /// </summary>
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si la conexion esta activa
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navegacion
    public User? User { get; set; }
    public ICollection<ReservationCalendarEvent>? CalendarEvents { get; set; }
}
