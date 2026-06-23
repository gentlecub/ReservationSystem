namespace SmartBookAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailVerificationAsync(string email, string fullName, string token);
    Task<bool> SendPasswordResetAsync(string email, string fullName, string token);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);
}
