namespace SmartBookAPI.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "SmartBook";
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
