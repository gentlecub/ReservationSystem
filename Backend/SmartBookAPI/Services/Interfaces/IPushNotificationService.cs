using SmartBookAPI.DTOs.Notifications;

namespace SmartBookAPI.Services.Interfaces;

/// <summary>
/// Servicio para enviar notificaciones push en tiempo real vía SignalR
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Envía una notificación push a un usuario específico
    /// </summary>
    Task SendToUserAsync(int userId, string notificationType, object payload);

    /// <summary>
    /// Envía notificación de reserva creada
    /// </summary>
    Task SendReservationCreatedAsync(int userId, ReservationDetails details);

    /// <summary>
    /// Envía notificación de reserva confirmada
    /// </summary>
    Task SendReservationConfirmedAsync(int userId, ReservationDetails details);

    /// <summary>
    /// Envía notificación de reserva cancelada
    /// </summary>
    Task SendReservationCancelledAsync(int userId, ReservationDetails details, string cancelledBy);

    /// <summary>
    /// Envía notificación de reserva modificada
    /// </summary>
    Task SendReservationModifiedAsync(int userId, ReservationDetails details, string changes);

    /// <summary>
    /// Envía recordatorio de reserva
    /// </summary>
    Task SendReservationReminderAsync(int userId, ReservationDetails details);
}
