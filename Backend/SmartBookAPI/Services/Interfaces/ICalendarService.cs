using SmartBookAPI.Models;

namespace SmartBookAPI.Services.Interfaces;

/// <summary>
/// Servicio para integracion con calendarios externos (Google Calendar y Microsoft Outlook)
/// </summary>
public interface ICalendarService
{
    // ========== Autorizacion ==========

    /// <summary>
    /// Obtiene la URL de autorizacion para Google Calendar
    /// </summary>
    string GetGoogleAuthUrl(int userId);

    /// <summary>
    /// Obtiene la URL de autorizacion para Microsoft Outlook
    /// </summary>
    string GetMicrosoftAuthUrl(int userId);

    /// <summary>
    /// Procesa el callback de OAuth de Google y guarda los tokens
    /// </summary>
    Task<bool> HandleGoogleCallbackAsync(int userId, string code);

    /// <summary>
    /// Procesa el callback de OAuth de Microsoft y guarda los tokens
    /// </summary>
    Task<bool> HandleMicrosoftCallbackAsync(int userId, string code);

    // ========== Conexiones ==========

    /// <summary>
    /// Obtiene las conexiones de calendario de un usuario
    /// </summary>
    Task<List<CalendarConnection>> GetConnectionsAsync(int userId);

    /// <summary>
    /// Desconecta un calendario
    /// </summary>
    Task<bool> DisconnectAsync(int userId, string provider);

    // ========== Eventos ==========

    /// <summary>
    /// Crea un evento en todos los calendarios conectados del usuario
    /// </summary>
    Task CreateEventAsync(Reservation reservation);

    /// <summary>
    /// Actualiza un evento en todos los calendarios conectados
    /// </summary>
    Task UpdateEventAsync(Reservation reservation);

    /// <summary>
    /// Elimina un evento de todos los calendarios conectados
    /// </summary>
    Task DeleteEventAsync(Reservation reservation);
}
