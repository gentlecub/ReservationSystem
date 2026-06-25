using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartBookAPI.Configuration;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly GoogleCalendarSettings _googleSettings;
    private readonly MicrosoftCalendarSettings _microsoftSettings;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(
        ICalendarService calendarService,
        IOptions<GoogleCalendarSettings> googleSettings,
        IOptions<MicrosoftCalendarSettings> microsoftSettings,
        ILogger<CalendarController> logger)
    {
        _calendarService = calendarService;
        _googleSettings = googleSettings.Value;
        _microsoftSettings = microsoftSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las conexiones de calendario del usuario actual
    /// </summary>
    [HttpGet("connections")]
    public async Task<IActionResult> GetConnections()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var connections = await _calendarService.GetConnectionsAsync(userId.Value);

        var result = connections.Select(c => new
        {
            c.Provider,
            c.CalendarEmail,
            c.ConnectedAt,
            c.IsActive
        });

        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Obtiene la URL de autorizacion para Google Calendar
    /// </summary>
    [HttpGet("google/auth")]
    public IActionResult GetGoogleAuthUrl()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(_googleSettings.ClientId))
        {
            return BadRequest(new { success = false, message = "Google Calendar no está configurado" });
        }

        var authUrl = _calendarService.GetGoogleAuthUrl(userId.Value);
        return Ok(new { success = true, data = new { authUrl } });
    }

    /// <summary>
    /// Callback de OAuth de Google (redirige al frontend)
    /// </summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            // Decodificar el state para obtener el userId
            var userId = int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(state)));

            var success = await _calendarService.HandleGoogleCallbackAsync(userId, code);

            // Redirigir al frontend con el resultado
            var frontendUrl = GetFrontendUrl();
            var redirectUrl = success
                ? $"{frontendUrl}/profile?calendar=google&status=success"
                : $"{frontendUrl}/profile?calendar=google&status=error";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en callback de Google");
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/profile?calendar=google&status=error");
        }
    }

    /// <summary>
    /// Obtiene la URL de autorizacion para Microsoft Calendar
    /// </summary>
    [HttpGet("microsoft/auth")]
    public IActionResult GetMicrosoftAuthUrl()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(_microsoftSettings.ClientId))
        {
            return BadRequest(new { success = false, message = "Microsoft Calendar no está configurado" });
        }

        var authUrl = _calendarService.GetMicrosoftAuthUrl(userId.Value);
        return Ok(new { success = true, data = new { authUrl } });
    }

    /// <summary>
    /// Callback de OAuth de Microsoft (redirige al frontend)
    /// </summary>
    [HttpGet("microsoft/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MicrosoftCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            // Decodificar el state para obtener el userId
            var userId = int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(state)));

            var success = await _calendarService.HandleMicrosoftCallbackAsync(userId, code);

            // Redirigir al frontend con el resultado
            var frontendUrl = GetFrontendUrl();
            var redirectUrl = success
                ? $"{frontendUrl}/profile?calendar=microsoft&status=success"
                : $"{frontendUrl}/profile?calendar=microsoft&status=error";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en callback de Microsoft");
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/profile?calendar=microsoft&status=error");
        }
    }

    /// <summary>
    /// Desconecta un calendario
    /// </summary>
    [HttpDelete("{provider}")]
    public async Task<IActionResult> Disconnect(string provider)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (provider != "Google" && provider != "Microsoft")
        {
            return BadRequest(new { success = false, message = "Proveedor no válido" });
        }

        var success = await _calendarService.DisconnectAsync(userId.Value, provider);

        if (!success)
        {
            return NotFound(new { success = false, message = "No se encontró la conexión" });
        }

        return Ok(new { success = true, message = $"{provider} Calendar desconectado exitosamente" });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    private string GetFrontendUrl()
    {
        // En produccion usar variable de entorno, en desarrollo usar localhost
        return Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
    }
}
