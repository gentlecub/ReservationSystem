using SmartBookAPI.DTOs.Notifications;

namespace SmartBookAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailVerificationAsync(string email, string fullName, string token);
    Task<bool> SendPasswordResetAsync(string email, string fullName, string token);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);

    // Métodos de notificación de reservas
    Task<bool> SendReservationCreatedAsync(string email, string fullName, ReservationDetails details);
    Task<bool> SendReservationConfirmedAsync(string email, string fullName, ReservationDetails details);
    Task<bool> SendReservationCancelledAsync(string email, string fullName, ReservationDetails details);
    Task<bool> SendReservationReminderAsync(string email, string fullName, ReservationDetails details);
    Task<bool> SendReservationModifiedAsync(string email, string fullName, ReservationDetails details, string changeDescription);
}
