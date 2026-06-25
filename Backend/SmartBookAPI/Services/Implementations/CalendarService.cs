using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using SmartBookAPI.Configuration;
using SmartBookAPI.Data;
using SmartBookAPI.Models;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

/// <summary>
/// Servicio que maneja la integracion con Google Calendar y Microsoft Outlook
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly AppDbContext _context;
    private readonly GoogleCalendarSettings _googleSettings;
    private readonly MicrosoftCalendarSettings _microsoftSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CalendarService> _logger;

    private const string GoogleScope = "https://www.googleapis.com/auth/calendar.events";
    private const string MicrosoftScope = "Calendars.ReadWrite offline_access";

    public CalendarService(
        AppDbContext context,
        IOptions<GoogleCalendarSettings> googleSettings,
        IOptions<MicrosoftCalendarSettings> microsoftSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<CalendarService> logger)
    {
        _context = context;
        _googleSettings = googleSettings.Value;
        _microsoftSettings = microsoftSettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    #region Authorization URLs

    public string GetGoogleAuthUrl(int userId)
    {
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}"));

        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={_googleSettings.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(_googleSettings.RedirectUri)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString(GoogleScope)}" +
               $"&access_type=offline" +
               $"&prompt=consent" +
               $"&state={state}";
    }

    public string GetMicrosoftAuthUrl(int userId)
    {
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}"));

        return $"https://login.microsoftonline.com/{_microsoftSettings.TenantId}/oauth2/v2.0/authorize?" +
               $"client_id={_microsoftSettings.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(_microsoftSettings.RedirectUri)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString(MicrosoftScope)}" +
               $"&state={state}";
    }

    #endregion

    #region OAuth Callbacks

    public async Task<bool> HandleGoogleCallbackAsync(int userId, string code)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Intercambiar codigo por tokens
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _googleSettings.ClientId,
                ["client_secret"] = _googleSettings.ClientSecret,
                ["redirect_uri"] = _googleSettings.RedirectUri,
                ["grant_type"] = "authorization_code"
            };

            var response = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error obteniendo tokens de Google: {Status}", response.StatusCode);
                return false;
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            var accessToken = tokenData.GetProperty("access_token").GetString()!;
            var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

            // Obtener info del usuario
            var userInfoResponse = await httpClient.GetAsync(
                $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);
            var email = userInfo.TryGetProperty("email", out var e) ? e.GetString() : null;

            // Guardar o actualizar conexion
            var connection = await _context.CalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Google");

            if (connection == null)
            {
                connection = new CalendarConnection
                {
                    UserId = userId,
                    Provider = "Google"
                };
                _context.CalendarConnections.Add(connection);
            }

            connection.AccessToken = accessToken;
            connection.RefreshToken = refreshToken ?? connection.RefreshToken;
            connection.TokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
            connection.CalendarId = "primary";
            connection.CalendarEmail = email;
            connection.IsActive = true;
            connection.ConnectedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {UserId} conecto Google Calendar", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando callback de Google para usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HandleMicrosoftCallbackAsync(int userId, string code)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Intercambiar codigo por tokens
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _microsoftSettings.ClientId,
                ["client_secret"] = _microsoftSettings.ClientSecret,
                ["redirect_uri"] = _microsoftSettings.RedirectUri,
                ["grant_type"] = "authorization_code",
                ["scope"] = MicrosoftScope
            };

            var response = await httpClient.PostAsync(
                $"https://login.microsoftonline.com/{_microsoftSettings.TenantId}/oauth2/v2.0/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error obteniendo tokens de Microsoft: {Status}", response.StatusCode);
                return false;
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            var accessToken = tokenData.GetProperty("access_token").GetString()!;
            var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

            // Obtener info del usuario
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var userInfoResponse = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);
            var email = userInfo.TryGetProperty("mail", out var m) ? m.GetString() :
                       userInfo.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null;

            // Guardar o actualizar conexion
            var connection = await _context.CalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Microsoft");

            if (connection == null)
            {
                connection = new CalendarConnection
                {
                    UserId = userId,
                    Provider = "Microsoft"
                };
                _context.CalendarConnections.Add(connection);
            }

            connection.AccessToken = accessToken;
            connection.RefreshToken = refreshToken ?? connection.RefreshToken;
            connection.TokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
            connection.CalendarEmail = email;
            connection.IsActive = true;
            connection.ConnectedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {UserId} conecto Microsoft Calendar", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando callback de Microsoft para usuario {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region Connections

    public async Task<List<CalendarConnection>> GetConnectionsAsync(int userId)
    {
        return await _context.CalendarConnections
            .Where(c => c.UserId == userId && c.IsActive)
            .ToListAsync();
    }

    public async Task<bool> DisconnectAsync(int userId, string provider)
    {
        var connection = await _context.CalendarConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == provider);

        if (connection == null)
            return false;

        connection.IsActive = false;
        connection.AccessToken = "";
        connection.RefreshToken = "";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UserId} desconecto {Provider} Calendar", userId, provider);
        return true;
    }

    #endregion

    #region Calendar Events

    public async Task CreateEventAsync(Reservation reservation)
    {
        try
        {
            var connections = await _context.CalendarConnections
                .Where(c => c.UserId == reservation.UserId && c.IsActive)
                .ToListAsync();

            foreach (var connection in connections)
            {
                try
                {
                    string? eventId = null;

                    if (connection.Provider == "Google")
                    {
                        eventId = await CreateGoogleEventAsync(connection, reservation);
                    }
                    else if (connection.Provider == "Microsoft")
                    {
                        eventId = await CreateMicrosoftEventAsync(connection, reservation);
                    }

                    if (!string.IsNullOrEmpty(eventId))
                    {
                        _context.ReservationCalendarEvents.Add(new ReservationCalendarEvent
                        {
                            ReservationId = reservation.ReservationId,
                            CalendarConnectionId = connection.Id,
                            ExternalEventId = eventId
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creando evento en {Provider} para reserva {ReservationId}",
                        connection.Provider, reservation.ReservationId);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando eventos de calendario para reserva {ReservationId}",
                reservation.ReservationId);
        }
    }

    public async Task UpdateEventAsync(Reservation reservation)
    {
        try
        {
            var calendarEvents = await _context.ReservationCalendarEvents
                .Include(e => e.CalendarConnection)
                .Where(e => e.ReservationId == reservation.ReservationId)
                .ToListAsync();

            foreach (var calendarEvent in calendarEvents)
            {
                if (calendarEvent.CalendarConnection == null || !calendarEvent.CalendarConnection.IsActive)
                    continue;

                try
                {
                    if (calendarEvent.CalendarConnection.Provider == "Google")
                    {
                        await UpdateGoogleEventAsync(calendarEvent.CalendarConnection,
                            calendarEvent.ExternalEventId, reservation);
                    }
                    else if (calendarEvent.CalendarConnection.Provider == "Microsoft")
                    {
                        await UpdateMicrosoftEventAsync(calendarEvent.CalendarConnection,
                            calendarEvent.ExternalEventId, reservation);
                    }

                    calendarEvent.UpdatedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error actualizando evento {EventId} en {Provider}",
                        calendarEvent.ExternalEventId, calendarEvent.CalendarConnection.Provider);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando eventos de calendario para reserva {ReservationId}",
                reservation.ReservationId);
        }
    }

    public async Task DeleteEventAsync(Reservation reservation)
    {
        try
        {
            var calendarEvents = await _context.ReservationCalendarEvents
                .Include(e => e.CalendarConnection)
                .Where(e => e.ReservationId == reservation.ReservationId)
                .ToListAsync();

            foreach (var calendarEvent in calendarEvents)
            {
                if (calendarEvent.CalendarConnection == null || !calendarEvent.CalendarConnection.IsActive)
                    continue;

                try
                {
                    if (calendarEvent.CalendarConnection.Provider == "Google")
                    {
                        await DeleteGoogleEventAsync(calendarEvent.CalendarConnection,
                            calendarEvent.ExternalEventId);
                    }
                    else if (calendarEvent.CalendarConnection.Provider == "Microsoft")
                    {
                        await DeleteMicrosoftEventAsync(calendarEvent.CalendarConnection,
                            calendarEvent.ExternalEventId);
                    }

                    _context.ReservationCalendarEvents.Remove(calendarEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error eliminando evento {EventId} en {Provider}",
                        calendarEvent.ExternalEventId, calendarEvent.CalendarConnection.Provider);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando eventos de calendario para reserva {ReservationId}",
                reservation.ReservationId);
        }
    }

    #endregion

    #region Google Calendar Implementation

    private async Task<string?> CreateGoogleEventAsync(CalendarConnection connection, Reservation reservation)
    {
        var service = await GetGoogleCalendarServiceAsync(connection);
        if (service == null) return null;

        var newEvent = CreateGoogleEventObject(reservation);

        var createdEvent = await service.Events.Insert(newEvent, "primary").ExecuteAsync();
        return createdEvent.Id;
    }

    private async Task UpdateGoogleEventAsync(CalendarConnection connection, string eventId, Reservation reservation)
    {
        var service = await GetGoogleCalendarServiceAsync(connection);
        if (service == null) return;

        var updatedEvent = CreateGoogleEventObject(reservation);
        await service.Events.Update(updatedEvent, "primary", eventId).ExecuteAsync();
    }

    private async Task DeleteGoogleEventAsync(CalendarConnection connection, string eventId)
    {
        var service = await GetGoogleCalendarServiceAsync(connection);
        if (service == null) return;

        await service.Events.Delete("primary", eventId).ExecuteAsync();
    }

    private async Task<Google.Apis.Calendar.v3.CalendarService?> GetGoogleCalendarServiceAsync(CalendarConnection connection)
    {
        // Refrescar token si es necesario
        if (connection.TokenExpiry <= DateTime.UtcNow.AddMinutes(5))
        {
            var refreshed = await RefreshGoogleTokenAsync(connection);
            if (!refreshed) return null;
        }

        var credential = GoogleCredential.FromAccessToken(connection.AccessToken);

        return new Google.Apis.Calendar.v3.CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "SmartBook"
        });
    }

    private async Task<bool> RefreshGoogleTokenAsync(CalendarConnection connection)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var tokenRequest = new Dictionary<string, string>
            {
                ["refresh_token"] = connection.RefreshToken,
                ["client_id"] = _googleSettings.ClientId,
                ["client_secret"] = _googleSettings.ClientSecret,
                ["grant_type"] = "refresh_token"
            };

            var response = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode) return false;

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            connection.AccessToken = tokenData.GetProperty("access_token").GetString()!;
            connection.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.GetProperty("expires_in").GetInt32());

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refrescando token de Google");
            return false;
        }
    }

    private Google.Apis.Calendar.v3.Data.Event CreateGoogleEventObject(Reservation reservation)
    {
        var startDateTime = reservation.Date.ToDateTime(reservation.StartTime);
        var endDateTime = reservation.Date.ToDateTime(reservation.EndTime);

        return new Google.Apis.Calendar.v3.Data.Event
        {
            Summary = $"Reserva - {reservation.Resource?.Name ?? "Recurso"}",
            Location = reservation.Resource?.Location,
            Description = $"Reserva en SmartBook\nEstado: {reservation.Status}\nRecurso: {reservation.Resource?.Name}",
            Start = new EventDateTime
            {
                DateTime = startDateTime,
                TimeZone = "America/Lima"
            },
            End = new EventDateTime
            {
                DateTime = endDateTime,
                TimeZone = "America/Lima"
            },
            Reminders = new Google.Apis.Calendar.v3.Data.Event.RemindersData
            {
                UseDefault = false,
                Overrides = new List<EventReminder>
                {
                    new EventReminder { Method = "popup", Minutes = 60 },
                    new EventReminder { Method = "email", Minutes = 1440 }
                }
            }
        };
    }

    #endregion

    #region Microsoft Calendar Implementation

    private async Task<string?> CreateMicrosoftEventAsync(CalendarConnection connection, Reservation reservation)
    {
        var graphClient = await GetMicrosoftGraphClientAsync(connection);
        if (graphClient == null) return null;

        var newEvent = CreateMicrosoftEventObject(reservation);

        var createdEvent = await graphClient.Me.Calendar.Events.PostAsync(newEvent);
        return createdEvent?.Id;
    }

    private async Task UpdateMicrosoftEventAsync(CalendarConnection connection, string eventId, Reservation reservation)
    {
        var graphClient = await GetMicrosoftGraphClientAsync(connection);
        if (graphClient == null) return;

        var updatedEvent = CreateMicrosoftEventObject(reservation);
        await graphClient.Me.Calendar.Events[eventId].PatchAsync(updatedEvent);
    }

    private async Task DeleteMicrosoftEventAsync(CalendarConnection connection, string eventId)
    {
        var graphClient = await GetMicrosoftGraphClientAsync(connection);
        if (graphClient == null) return;

        await graphClient.Me.Calendar.Events[eventId].DeleteAsync();
    }

    private async Task<GraphServiceClient?> GetMicrosoftGraphClientAsync(CalendarConnection connection)
    {
        // Refrescar token si es necesario
        if (connection.TokenExpiry <= DateTime.UtcNow.AddMinutes(5))
        {
            var refreshed = await RefreshMicrosoftTokenAsync(connection);
            if (!refreshed) return null;
        }

        var tokenCredential = new AccessTokenCredential(connection.AccessToken);
        return new GraphServiceClient(tokenCredential);
    }

    private async Task<bool> RefreshMicrosoftTokenAsync(CalendarConnection connection)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var tokenRequest = new Dictionary<string, string>
            {
                ["refresh_token"] = connection.RefreshToken,
                ["client_id"] = _microsoftSettings.ClientId,
                ["client_secret"] = _microsoftSettings.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["scope"] = MicrosoftScope
            };

            var response = await httpClient.PostAsync(
                $"https://login.microsoftonline.com/{_microsoftSettings.TenantId}/oauth2/v2.0/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode) return false;

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            connection.AccessToken = tokenData.GetProperty("access_token").GetString()!;
            connection.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.GetProperty("expires_in").GetInt32());

            if (tokenData.TryGetProperty("refresh_token", out var newRefreshToken))
            {
                connection.RefreshToken = newRefreshToken.GetString()!;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refrescando token de Microsoft");
            return false;
        }
    }

    private Microsoft.Graph.Models.Event CreateMicrosoftEventObject(Reservation reservation)
    {
        var startDateTime = reservation.Date.ToDateTime(reservation.StartTime);
        var endDateTime = reservation.Date.ToDateTime(reservation.EndTime);

        return new Microsoft.Graph.Models.Event
        {
            Subject = $"Reserva - {reservation.Resource?.Name ?? "Recurso"}",
            Location = new Location { DisplayName = reservation.Resource?.Location },
            Body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = $"Reserva en SmartBook\nEstado: {reservation.Status}\nRecurso: {reservation.Resource?.Name}"
            },
            Start = new DateTimeTimeZone
            {
                DateTime = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = "America/Lima"
            },
            End = new DateTimeTimeZone
            {
                DateTime = endDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = "America/Lima"
            },
            ReminderMinutesBeforeStart = 60
        };
    }

    #endregion
}

/// <summary>
/// Credencial simple para usar con Microsoft Graph
/// </summary>
internal class AccessTokenCredential : Azure.Core.TokenCredential
{
    private readonly string _accessToken;

    public AccessTokenCredential(string accessToken)
    {
        _accessToken = accessToken;
    }

    public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext,
        System.Threading.CancellationToken cancellationToken)
    {
        return new Azure.Core.AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1));
    }

    public override ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext,
        System.Threading.CancellationToken cancellationToken)
    {
        return new ValueTask<Azure.Core.AccessToken>(GetToken(requestContext, cancellationToken));
    }
}
