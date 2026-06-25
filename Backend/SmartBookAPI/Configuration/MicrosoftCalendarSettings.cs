namespace SmartBookAPI.Configuration;

/// <summary>
/// Configuracion para Microsoft Graph API (Outlook Calendar)
/// </summary>
public class MicrosoftCalendarSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
    public string RedirectUri { get; set; } = string.Empty;
}
