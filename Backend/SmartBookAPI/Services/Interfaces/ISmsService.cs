using SmartBookAPI.DTOs.Notifications;

namespace SmartBookAPI.Services.Interfaces;

public interface ISmsService
{
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);

    // Métodos de notificación de reservas por SMS
    Task<bool> SendReservationCreatedSmsAsync(string phoneNumber, ReservationDetails details);
    Task<bool> SendReservationConfirmedSmsAsync(string phoneNumber, ReservationDetails details);
    Task<bool> SendReservationCancelledSmsAsync(string phoneNumber, ReservationDetails details);
    Task<bool> SendReservationReminderSmsAsync(string phoneNumber, ReservationDetails details);
}
