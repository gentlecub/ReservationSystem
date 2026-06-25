using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartBookAPI.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real.
/// Los clientes se conectan aquí para recibir notificaciones push.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cuando un cliente se conecta, lo agregamos a un grupo basado en su UserId.
    /// Esto permite enviar notificaciones específicas a cada usuario.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar al grupo del usuario
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("Usuario {UserId} conectado al hub de notificaciones", userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Cuando un cliente se desconecta, lo removemos de su grupo.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("Usuario {UserId} desconectado del hub de notificaciones", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
