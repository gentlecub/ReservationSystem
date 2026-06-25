namespace SmartBookAPI.DTOs.Notifications;

/// <summary>
/// DTO para las preferencias de notificación del usuario
/// </summary>
public class NotificationPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = true;
}
