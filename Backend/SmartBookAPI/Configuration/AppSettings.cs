namespace SmartBookAPI.Configuration;

public class AppSettings
{
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public string GoogleClientId { get; set; } = string.Empty;
}
