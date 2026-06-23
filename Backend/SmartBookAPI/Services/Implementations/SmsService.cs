using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartBookAPI.Configuration;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class SmsService : ISmsService
{
    private readonly TwilioSettings _twilioSettings;
    private readonly ILogger<SmsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;

    public SmsService(
        IOptions<TwilioSettings> twilioSettings,
        ILogger<SmsService> logger,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment)
    {
        _twilioSettings = twilioSettings.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _environment = environment;
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        try
        {
            var accountSid = GetAccountSid();
            var authToken = GetAuthToken();
            var fromNumber = GetPhoneNumber();

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("Twilio credentials not configured. SMS not sent to {PhoneNumber}", phoneNumber);
                return false;
            }

            var message = $"Tu código de verificación de SmartBook es: {code}. Expira en 10 minutos.";

            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("From", fromNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send SMS to {PhoneNumber}. Response: {Response}", phoneNumber, responseBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private string GetAccountSid()
    {
        return _environment.IsProduction()
            ? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? string.Empty
            : _twilioSettings.AccountSid;
    }

    private string GetAuthToken()
    {
        return _environment.IsProduction()
            ? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? string.Empty
            : _twilioSettings.AuthToken;
    }

    private string GetPhoneNumber()
    {
        return _environment.IsProduction()
            ? Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ?? string.Empty
            : _twilioSettings.PhoneNumber;
    }
}
