using Microsoft.AspNetCore.SignalR;
using SmartBookAPI.DTOs.Notifications;
using SmartBookAPI.Hubs;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

/// <summary>
/// Implementación del servicio de notificaciones push usando SignalR.
/// Envía notificaciones en tiempo real a los clientes conectados.
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<PushNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(int userId, string notificationType, object payload)
    {
        try
        {
            var groupName = $"user_{userId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Type = notificationType,
                Payload = payload,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Push notification sent to user {UserId}: {Type}", userId, notificationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
        }
    }

    public async Task SendReservationCreatedAsync(int userId, ReservationDetails details)
    {
        await SendToUserAsync(userId, "ReservationCreated", new
        {
            Title = "Reserva Creada",
            Message = $"Tu reserva para {details.ResourceName} el {details.Date:dd/MM/yyyy} a las {details.StartTime:HH:mm} ha sido registrada.",
            Details = details
        });
    }

    public async Task SendReservationConfirmedAsync(int userId, ReservationDetails details)
    {
        await SendToUserAsync(userId, "ReservationConfirmed", new
        {
            Title = "Reserva Confirmada",
            Message = $"Tu reserva para {details.ResourceName} el {details.Date:dd/MM/yyyy} a las {details.StartTime:HH:mm} ha sido confirmada.",
            Details = details
        });
    }

    public async Task SendReservationCancelledAsync(int userId, ReservationDetails details, string cancelledBy)
    {
        var message = cancelledBy == "admin"
            ? $"Tu reserva para {details.ResourceName} el {details.Date:dd/MM/yyyy} ha sido cancelada por el administrador."
            : $"Tu reserva para {details.ResourceName} el {details.Date:dd/MM/yyyy} ha sido cancelada.";

        await SendToUserAsync(userId, "ReservationCancelled", new
        {
            Title = "Reserva Cancelada",
            Message = message,
            CancelledBy = cancelledBy,
            Details = details
        });
    }

    public async Task SendReservationModifiedAsync(int userId, ReservationDetails details, string changes)
    {
        await SendToUserAsync(userId, "ReservationModified", new
        {
            Title = "Reserva Modificada",
            Message = $"Tu reserva ha sido modificada. Cambios: {changes}",
            Changes = changes,
            Details = details
        });
    }

    public async Task SendReservationReminderAsync(int userId, ReservationDetails details)
    {
        await SendToUserAsync(userId, "ReservationReminder", new
        {
            Title = "Recordatorio de Reserva",
            Message = $"Recuerda que tienes una reserva para {details.ResourceName} mañana {details.Date:dd/MM/yyyy} a las {details.StartTime:HH:mm}.",
            Details = details
        });
    }
}
