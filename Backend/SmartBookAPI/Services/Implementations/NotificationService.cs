using Microsoft.Extensions.DependencyInjection;
using SmartBookAPI.Data;
using SmartBookAPI.DTOs.Notifications;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

/// <summary>
/// Servicio unificado de notificaciones que orquesta email, SMS y push
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushNotificationService _pushService;
    private readonly IUserRepository _userRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        ISmsService smsService,
        IPushNotificationService pushService,
        IUserRepository userRepository,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
        _userRepository = userRepository;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task NotifyReservationCreatedAsync(Reservation reservation)
    {
        await SendNotificationAsync(reservation, "Created", async (user, details) =>
        {
            var emailSuccess = false;
            var smsSuccess = false;

            // Enviar notificación push (siempre, si el usuario está conectado)
            await _pushService.SendReservationCreatedAsync(user.UserId, details);
            await LogNotificationAsync(reservation.ReservationId, user.UserId, "Created", "Push", true);

            // Enviar email si está verificado y las notificaciones por email están habilitadas
            if (user.EmailVerified && user.EmailNotifications)
            {
                emailSuccess = await _emailService.SendReservationCreatedAsync(user.Email, user.FullName, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Created", "Email", emailSuccess);
            }

            // Enviar SMS si está verificado y las notificaciones por SMS están habilitadas
            if (user.PhoneVerified && !string.IsNullOrEmpty(user.PhoneNumber) && user.SmsNotifications)
            {
                smsSuccess = await _smsService.SendReservationCreatedSmsAsync(user.PhoneNumber, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Created", "SMS", smsSuccess);
            }

            return emailSuccess || smsSuccess;
        });
    }

    public async Task NotifyReservationConfirmedAsync(Reservation reservation)
    {
        await SendNotificationAsync(reservation, "Confirmed", async (user, details) =>
        {
            var emailSuccess = false;
            var smsSuccess = false;

            // Enviar notificación push
            await _pushService.SendReservationConfirmedAsync(user.UserId, details);
            await LogNotificationAsync(reservation.ReservationId, user.UserId, "Confirmed", "Push", true);

            if (user.EmailVerified && user.EmailNotifications)
            {
                emailSuccess = await _emailService.SendReservationConfirmedAsync(user.Email, user.FullName, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Confirmed", "Email", emailSuccess);
            }

            if (user.PhoneVerified && !string.IsNullOrEmpty(user.PhoneNumber) && user.SmsNotifications)
            {
                smsSuccess = await _smsService.SendReservationConfirmedSmsAsync(user.PhoneNumber, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Confirmed", "SMS", smsSuccess);
            }

            return emailSuccess || smsSuccess;
        });
    }

    public async Task NotifyReservationCancelledAsync(Reservation reservation, string cancelledBy)
    {
        await SendNotificationAsync(reservation, "Cancelled", async (user, details) =>
        {
            var emailSuccess = false;
            var smsSuccess = false;

            // Enviar notificación push
            await _pushService.SendReservationCancelledAsync(user.UserId, details, cancelledBy);
            await LogNotificationAsync(reservation.ReservationId, user.UserId, "Cancelled", "Push", true);

            if (user.EmailVerified && user.EmailNotifications)
            {
                emailSuccess = await _emailService.SendReservationCancelledAsync(user.Email, user.FullName, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Cancelled", "Email", emailSuccess);
            }

            if (user.PhoneVerified && !string.IsNullOrEmpty(user.PhoneNumber) && user.SmsNotifications)
            {
                smsSuccess = await _smsService.SendReservationCancelledSmsAsync(user.PhoneNumber, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Cancelled", "SMS", smsSuccess);
            }

            return emailSuccess || smsSuccess;
        });
    }

    public async Task NotifyReservationModifiedAsync(Reservation reservation, string changes)
    {
        await SendNotificationAsync(reservation, "Modified", async (user, details) =>
        {
            var emailSuccess = false;

            // Enviar notificación push
            await _pushService.SendReservationModifiedAsync(user.UserId, details, changes);
            await LogNotificationAsync(reservation.ReservationId, user.UserId, "Modified", "Push", true);

            if (user.EmailVerified && user.EmailNotifications)
            {
                emailSuccess = await _emailService.SendReservationModifiedAsync(user.Email, user.FullName, details, changes);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Modified", "Email", emailSuccess);
            }

            return emailSuccess;
        });
    }

    public async Task SendReminderAsync(Reservation reservation)
    {
        await SendNotificationAsync(reservation, "Reminder", async (user, details) =>
        {
            var emailSuccess = false;
            var smsSuccess = false;

            // Enviar notificación push
            await _pushService.SendReservationReminderAsync(user.UserId, details);
            await LogNotificationAsync(reservation.ReservationId, user.UserId, "Reminder", "Push", true);

            if (user.EmailVerified && user.EmailNotifications)
            {
                emailSuccess = await _emailService.SendReservationReminderAsync(user.Email, user.FullName, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Reminder", "Email", emailSuccess);
            }

            if (user.PhoneVerified && !string.IsNullOrEmpty(user.PhoneNumber) && user.SmsNotifications)
            {
                smsSuccess = await _smsService.SendReservationReminderSmsAsync(user.PhoneNumber, details);
                await LogNotificationAsync(reservation.ReservationId, user.UserId, "Reminder", "SMS", smsSuccess);
            }

            return emailSuccess || smsSuccess;
        });
    }

    private async Task SendNotificationAsync(
        Reservation reservation,
        string notificationType,
        Func<User, ReservationDetails, Task<bool>> sendAction)
    {
        try
        {
            // Obtener usuario
            var user = await _userRepository.GetByIdAsync(reservation.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for reservation {ReservationId}", reservation.ReservationId);
                return;
            }

            // Crear detalles de la reserva
            var details = new ReservationDetails
            {
                ReservationId = reservation.ReservationId,
                ResourceName = reservation.Resource?.Name ?? "Recurso desconocido",
                ResourceLocation = reservation.Resource?.Location,
                Date = reservation.Date,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status
            };

            // Ejecutar la acción de envío
            await sendAction(user, details);
        }
        catch (Exception ex)
        {
            // No bloquear la operación principal si falla la notificación
            _logger.LogError(ex, "Error sending {NotificationType} notification for reservation {ReservationId}",
                notificationType, reservation.ReservationId);
        }
    }

    private async Task LogNotificationAsync(int? reservationId, int userId, string type, string channel, bool success, string? errorMessage = null)
    {
        try
        {
            // Crear un nuevo scope para evitar problemas con DbContext disposed
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new NotificationLog
            {
                ReservationId = reservationId,
                UserId = userId,
                Type = type,
                Channel = channel,
                Success = success,
                ErrorMessage = errorMessage,
                SentAt = DateTime.UtcNow
            };

            dbContext.NotificationLogs.Add(log);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging notification for reservation {ReservationId}", reservationId);
        }
    }
}
