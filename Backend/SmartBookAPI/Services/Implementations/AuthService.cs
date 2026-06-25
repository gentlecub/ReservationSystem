using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartBookAPI.Configuration;
using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Auth;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSettings _appSettings;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IEmailService emailService,
        ISmsService smsService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> appSettings)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _emailService = emailService;
        _smsService = smsService;
        _configuration = configuration;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _appSettings = appSettings.Value;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return ApiResponse<AuthResponse>.Fail("El email ya está registrado");
        }

        var clientRole = await _roleRepository.GetByNameAsync("Client");
        if (clientRole == null)
        {
            return ApiResponse<AuthResponse>.Fail("Error de configuración: rol Client no encontrado");
        }

        var emailVerificationToken = GenerateSecureToken();

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = clientRole.RoleId,
            CreatedAt = DateTime.UtcNow,
            AuthProvider = "Local",
            EmailVerified = false,
            PasswordResetToken = emailVerificationToken,
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        await _userRepository.CreateAsync(user);

        // Enviar email de verificación
        await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, emailVerificationToken);

        var token = GenerateJwtToken(user);
        var response = CreateAuthResponse(user, token);

        return ApiResponse<AuthResponse>.Ok(response, "Usuario registrado. Por favor verifica tu email.");
    }

    public async Task<ApiResponse<AuthResponse>> RegisterWithPhoneAsync(RegisterWithPhoneRequest request)
    {
        if (await _userRepository.PhoneExistsAsync(request.PhoneNumber))
        {
            return ApiResponse<AuthResponse>.Fail("El número de teléfono ya está registrado");
        }

        var clientRole = await _roleRepository.GetByNameAsync("Client");
        if (clientRole == null)
        {
            return ApiResponse<AuthResponse>.Fail("Error de configuración: rol Client no encontrado");
        }

        var verificationCode = GenerateVerificationCode();

        var user = new User
        {
            FullName = request.FullName,
            Email = $"{request.PhoneNumber}@phone.smartbook.local",
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = clientRole.RoleId,
            CreatedAt = DateTime.UtcNow,
            AuthProvider = "Phone",
            PhoneVerified = false,
            PhoneVerificationCode = verificationCode,
            PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10)
        };

        await _userRepository.CreateAsync(user);

        // Enviar SMS con código
        await _smsService.SendVerificationCodeAsync(request.PhoneNumber, verificationCode);

        var token = GenerateJwtToken(user);
        var response = CreateAuthResponse(user, token);

        return ApiResponse<AuthResponse>.Ok(response, "Usuario registrado. Verifica tu teléfono con el código SMS.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("La cuenta está desactivada");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("Esta cuenta no tiene contraseña configurada. Usa Google para iniciar sesión o recupera tu contraseña para establecer una.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("Credenciales inválidas");
        }

        var token = GenerateJwtToken(user);
        var response = CreateAuthResponse(user, token);

        return ApiResponse<AuthResponse>.Ok(response, "Inicio de sesión exitoso");
    }

    public async Task<ApiResponse<AuthResponse>> GoogleAuthAsync(ExternalAuthRequest request)
    {
        try
        {
            // Validar token con Google
            var googleUser = await ValidateGoogleTokenAsync(request.AccessToken);
            if (googleUser == null)
            {
                return ApiResponse<AuthResponse>.Fail("Token de Google inválido");
            }

            // Buscar usuario existente por ExternalId
            var user = await _userRepository.GetByExternalIdAsync(googleUser.Id, "Google");

            if (user == null)
            {
                // Verificar si el email ya existe con otra cuenta
                user = await _userRepository.GetByEmailAsync(googleUser.Email);
                if (user != null)
                {
                    // Vincular cuenta existente con Google
                    user.ExternalId = googleUser.Id;
                    user.AuthProvider = "Google";
                    user.EmailVerified = true;
                    user.ProfilePhotoUrl = googleUser.Picture;
                    await _userRepository.UpdateAsync(user);
                }
                else
                {
                    // Crear nuevo usuario
                    var clientRole = await _roleRepository.GetByNameAsync("Client");
                    if (clientRole == null)
                    {
                        return ApiResponse<AuthResponse>.Fail("Error de configuración: rol Client no encontrado");
                    }

                    user = new User
                    {
                        FullName = googleUser.Name,
                        Email = googleUser.Email,
                        PasswordHash = null,
                        RoleId = clientRole.RoleId,
                        CreatedAt = DateTime.UtcNow,
                        AuthProvider = "Google",
                        ExternalId = googleUser.Id,
                        EmailVerified = true,
                        ProfilePhotoUrl = googleUser.Picture
                    };

                    await _userRepository.CreateAsync(user);
                }
            }

            if (!user.IsActive)
            {
                return ApiResponse<AuthResponse>.Fail("La cuenta está desactivada");
            }

            var token = GenerateJwtToken(user);
            var response = CreateAuthResponse(user, token);

            return ApiResponse<AuthResponse>.Ok(response, "Autenticación con Google exitosa");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponse>.Fail($"Error en autenticación con Google: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> VerifyPhoneAsync(VerifyPhoneRequest request)
    {
        var user = await _userRepository.GetByPhoneAsync(request.PhoneNumber);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Usuario no encontrado");
        }

        if (user.PhoneVerified)
        {
            return ApiResponse<string>.Fail("El teléfono ya está verificado");
        }

        if (user.PhoneVerificationCode != request.Code)
        {
            return ApiResponse<string>.Fail("Código de verificación incorrecto");
        }

        if (user.PhoneVerificationCodeExpiry < DateTime.UtcNow)
        {
            return ApiResponse<string>.Fail("El código ha expirado. Solicita uno nuevo.");
        }

        user.PhoneVerified = true;
        user.PhoneVerificationCode = null;
        user.PhoneVerificationCodeExpiry = null;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.Ok("Teléfono verificado exitosamente");
    }

    public async Task<ApiResponse<string>> ResendSmsCodeAsync(ResendSmsRequest request)
    {
        var user = await _userRepository.GetByPhoneAsync(request.PhoneNumber);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Usuario no encontrado");
        }

        if (user.PhoneVerified)
        {
            return ApiResponse<string>.Fail("El teléfono ya está verificado");
        }

        var verificationCode = GenerateVerificationCode();
        user.PhoneVerificationCode = verificationCode;
        user.PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        await _userRepository.UpdateAsync(user);

        await _smsService.SendVerificationCodeAsync(request.PhoneNumber, verificationCode);

        return ApiResponse<string>.Ok("Código SMS reenviado exitosamente");
    }

    public async Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Token inválido o expirado");
        }

        if (user.EmailVerified)
        {
            return ApiResponse<string>.Fail("El email ya está verificado");
        }

        user.EmailVerified = true;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userRepository.UpdateAsync(user);

        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

        return ApiResponse<string>.Ok("Email verificado exitosamente");
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Por seguridad, no revelamos si el email existe
            return ApiResponse<string>.Ok("Si el email existe, recibirás instrucciones para recuperar tu contraseña");
        }

        // Permitir establecer contraseña para cuentas de Google también
        var resetToken = GenerateSecureToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user);

        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetToken);

        return ApiResponse<string>.Ok("Si el email existe, recibirás instrucciones para recuperar tu contraseña");
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Token inválido o expirado");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.Ok("Contraseña restablecida exitosamente");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        string secretKey;
        if (_environment.IsProduction())
        {
            secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET no está configurada");
        }
        else
        {
            secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey no configurado");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Client"),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AuthResponse CreateAuthResponse(User user, string token)
    {
        var expirationMinutes = int.Parse(
            _configuration.GetSection("JwtSettings")["ExpirationMinutes"] ?? "60");

        return new AuthResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Client",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Primero intentar validar como ID Token (Google Sign-In GSI)
            var idTokenResponse = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={token}");
            if (idTokenResponse.IsSuccessStatusCode)
            {
                var content = await idTokenResponse.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleIdTokenInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenInfo != null)
                {
                    return new GoogleUserInfo
                    {
                        Id = tokenInfo.Sub,
                        Email = tokenInfo.Email,
                        Name = tokenInfo.Name,
                        Picture = tokenInfo.Picture ?? string.Empty
                    };
                }
            }

            // Si falla, intentar como Access Token (OAuth tradicional)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            if (userInfoResponse.IsSuccessStatusCode)
            {
                var content = await userInfoResponse.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    private class GoogleIdTokenInfo
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
    }
}
