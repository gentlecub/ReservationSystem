namespace SmartBookAPI.Models;

/// <summary>
/// Vincula una reserva con un evento creado en un calendario externo
/// </summary>
public class ReservationCalendarEvent
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int CalendarConnectionId { get; set; }

    /// <summary>
    /// ID del evento en el calendario externo (Google/Outlook)
    /// </summary>
    public string ExternalEventId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creacion del evento
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ultima actualizacion del evento
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navegacion
    public Reservation? Reservation { get; set; }
    public CalendarConnection? CalendarConnection { get; set; }
}
