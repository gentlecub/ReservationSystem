using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SmartBookAPI.Configuration;
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
