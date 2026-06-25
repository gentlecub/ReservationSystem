using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services;

/// <summary>
/// Servicio en segundo plano que envía recordatorios de reservas 24 horas antes
/// </summary>
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ReminderBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderBackgroundService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending reminders");
            }

            // Esperar 1 hora antes de volver a verificar
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("ReminderBackgroundService is stopping");
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Obtener reservas confirmadas para mañana que no han recibido recordatorio
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var reservationsToRemind = await reservationRepository.GetConfirmedReservationsForDateWithoutReminderAsync(tomorrow);

        _logger.LogInformation("Found {Count} reservations to remind for {Date}",
            reservationsToRemind.Count(), tomorrow);

        foreach (var reservation in reservationsToRemind)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                // Enviar recordatorio
                await notificationService.SendReminderAsync(reservation);

                // Marcar como recordatorio enviado
                await reservationRepository.MarkReminderSentAsync(reservation.ReservationId);

                _logger.LogInformation("Reminder sent for reservation {ReservationId}", reservation.ReservationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for reservation {ReservationId}", reservation.ReservationId);
            }
        }
    }
}
