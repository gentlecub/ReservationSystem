namespace SmartBookAPI.Services.Interfaces;

public interface ISmsService
{
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
}
