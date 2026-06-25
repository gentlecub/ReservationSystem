using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SmartBookAPI.Configuration;
using SmartBookAPI.DTOs.Notifications;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly AppSettings _appSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IOptions<AppSettings> appSettings,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment)
    {
        _emailSettings = emailSettings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task<bool> SendEmailVerificationAsync(string email, string fullName, string token)
    {
        var verificationUrl = $"{GetFrontendUrl()}/verify-email?token={token}";
        var subject = "Verifica tu email - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Gracias por registrarte en SmartBook. Por favor verifica tu email haciendo clic en el siguiente enlace:</p>
                <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verificar Email</a></p>
                <p>O copia y pega este enlace en tu navegador:</p>
                <p>{verificationUrl}</p>
                <p>Este enlace expirará en 24 horas.</p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendPasswordResetAsync(string email, string fullName, string token)
    {
        var resetUrl = $"{GetFrontendUrl()}/reset-password?token={token}";
        var subject = "Recuperar contraseña - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el siguiente enlace:</p>
                <p><a href='{resetUrl}' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Restablecer Contraseña</a></p>
                <p>O copia y pega este enlace en tu navegador:</p>
                <p>{resetUrl}</p>
                <p>Este enlace expirará en 1 hora.</p>
                <p>Si no solicitaste este cambio, ignora este correo.</p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
    {
        var subject = "Bienvenido a SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>¡Bienvenido {fullName}!</h2>
                <p>Tu cuenta ha sido verificada exitosamente.</p>
                <p>Ya puedes comenzar a hacer reservas en SmartBook.</p>
                <p><a href='{GetFrontendUrl()}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ir a SmartBook</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReservationCreatedAsync(string email, string fullName, ReservationDetails details)
    {
        var subject = "Tu reserva está pendiente de confirmación - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Tu reserva ha sido creada y está pendiente de confirmación.</p>

                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #333;'>Detalles de tu reserva</h3>
                    <p><strong>Recurso:</strong> {details.ResourceName}</p>
                    {(string.IsNullOrEmpty(details.ResourceLocation) ? "" : $"<p><strong>Ubicación:</strong> {details.ResourceLocation}</p>")}
                    <p><strong>Fecha:</strong> {details.Date:dd/MM/yyyy}</p>
                    <p><strong>Horario:</strong> {details.StartTime:HH:mm} - {details.EndTime:HH:mm}</p>
                    <p><strong>Estado:</strong> <span style='color: #FFA500;'>Pendiente</span></p>
                </div>

                <p>Te notificaremos cuando tu reserva sea confirmada.</p>

                <p><a href='{GetFrontendUrl()}/reservations' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ver mis reservas</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReservationConfirmedAsync(string email, string fullName, ReservationDetails details)
    {
        var subject = "¡Tu reserva ha sido confirmada! - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>¡Excelentes noticias! Tu reserva ha sido confirmada.</p>

                <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #2e7d32;'>Detalles de tu reserva confirmada</h3>
                    <p><strong>Recurso:</strong> {details.ResourceName}</p>
                    {(string.IsNullOrEmpty(details.ResourceLocation) ? "" : $"<p><strong>Ubicación:</strong> {details.ResourceLocation}</p>")}
                    <p><strong>Fecha:</strong> {details.Date:dd/MM/yyyy}</p>
                    <p><strong>Horario:</strong> {details.StartTime:HH:mm} - {details.EndTime:HH:mm}</p>
                    <p><strong>Estado:</strong> <span style='color: #4CAF50;'>Confirmada</span></p>
                </div>

                <p>Por favor, llega puntualmente a tu cita.</p>

                <p><a href='{GetFrontendUrl()}/reservations' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ver mis reservas</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReservationCancelledAsync(string email, string fullName, ReservationDetails details)
    {
        var subject = "Tu reserva ha sido cancelada - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Tu reserva ha sido cancelada.</p>

                <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #c62828;'>Detalles de la reserva cancelada</h3>
                    <p><strong>Recurso:</strong> {details.ResourceName}</p>
                    {(string.IsNullOrEmpty(details.ResourceLocation) ? "" : $"<p><strong>Ubicación:</strong> {details.ResourceLocation}</p>")}
                    <p><strong>Fecha:</strong> {details.Date:dd/MM/yyyy}</p>
                    <p><strong>Horario:</strong> {details.StartTime:HH:mm} - {details.EndTime:HH:mm}</p>
                    <p><strong>Estado:</strong> <span style='color: #f44336;'>Cancelada</span></p>
                </div>

                <p>Si deseas realizar una nueva reserva, puedes hacerlo desde nuestra plataforma.</p>

                <p><a href='{GetFrontendUrl()}/resources' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Hacer nueva reserva</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReservationReminderAsync(string email, string fullName, ReservationDetails details)
    {
        var subject = "Recordatorio: Tu cita es mañana - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Te recordamos que tienes una reserva programada para mañana.</p>

                <div style='background-color: #e3f2fd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #1565c0;'>Detalles de tu reserva</h3>
                    <p><strong>Recurso:</strong> {details.ResourceName}</p>
                    {(string.IsNullOrEmpty(details.ResourceLocation) ? "" : $"<p><strong>Ubicación:</strong> {details.ResourceLocation}</p>")}
                    <p><strong>Fecha:</strong> {details.Date:dd/MM/yyyy}</p>
                    <p><strong>Horario:</strong> {details.StartTime:HH:mm} - {details.EndTime:HH:mm}</p>
                </div>

                <p><strong>Recuerda:</strong></p>
                <ul>
                    <li>Llega puntualmente a tu cita</li>
                    <li>Si no puedes asistir, por favor cancela tu reserva con anticipación</li>
                </ul>

                <p><a href='{GetFrontendUrl()}/reservations' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ver mis reservas</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReservationModifiedAsync(string email, string fullName, ReservationDetails details, string changeDescription)
    {
        var subject = "Tu reserva ha sido modificada - SmartBook";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Hola {fullName},</h2>
                <p>Tu reserva ha sido modificada.</p>

                <div style='background-color: #fff3e0; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #e65100;'>Cambios realizados</h3>
                    <p>{changeDescription}</p>
                </div>

                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #333;'>Detalles actualizados de tu reserva</h3>
                    <p><strong>Recurso:</strong> {details.ResourceName}</p>
                    {(string.IsNullOrEmpty(details.ResourceLocation) ? "" : $"<p><strong>Ubicación:</strong> {details.ResourceLocation}</p>")}
                    <p><strong>Fecha:</strong> {details.Date:dd/MM/yyyy}</p>
                    <p><strong>Horario:</strong> {details.StartTime:HH:mm} - {details.EndTime:HH:mm}</p>
                    <p><strong>Estado:</strong> {details.Status}</p>
                </div>

                <p><a href='{GetFrontendUrl()}/reservations' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ver mis reservas</a></p>
                <br/>
                <p>Saludos,<br/>El equipo de SmartBook</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var smtpPassword = GetSmtpPassword();
            if (string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("SMTP password not configured. Email not sent to {Email}", toEmail);
                return false;
            }

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, smtpPassword),
                EnableSsl = _emailSettings.EnableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            return false;
        }
    }

    private string GetSmtpPassword()
    {
        return _environment.IsProduction()
            ? Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty
            : _emailSettings.Password;
    }

    private string GetFrontendUrl()
    {
        return _environment.IsProduction()
            ? Environment.GetEnvironmentVariable("FRONTEND_URL") ?? _appSettings.FrontendUrl
            : _appSettings.FrontendUrl;
    }
}
