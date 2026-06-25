using SmartBookAPI.Models;

namespace SmartBookAPI.Services.Interfaces;

/// <summary>
/// Servicio unificado de notificaciones que orquesta email y SMS
/// </summary>
public interface INotificationService
{
    Task NotifyReservationCreatedAsync(Reservation reservation);
    Task NotifyReservationConfirmedAsync(Reservation reservation);
    Task NotifyReservationCancelledAsync(Reservation reservation, string cancelledBy);
    Task NotifyReservationModifiedAsync(Reservation reservation, string changes);
    Task SendReminderAsync(Reservation reservation);
}
