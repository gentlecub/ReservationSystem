using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Verificar si el email ya existe
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return ApiResponse<AuthResponse>.Fail("El email ya está registrado");
        }

        // Obtener el rol "Client" por defecto
        var clientRole = await _roleRepository.GetByNameAsync("Client");
        if (clientRole == null)
        {
            return ApiResponse<AuthResponse>.Fail("Error de configuración: rol Client no encontrado");
        }

        // Crear el usuario
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = clientRole.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Generar token y respuesta
        var token = GenerateJwtToken(user);
        var response = CreateAuthResponse(user, token);

        return ApiResponse<AuthResponse>.Ok(response, "Usuario registrado exitosamente");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("Credenciales inválidas");
        }

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("Credenciales inválidas");
        }

        // Generar token y respuesta
        var token = GenerateJwtToken(user);
        var response = CreateAuthResponse(user, token);

        return ApiResponse<AuthResponse>.Ok(response, "Inicio de sesión exitoso");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        string secretKey;
        if (_environment.IsProduction())
        {
            secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET no está configurada en las variables de entorno");
        }
        else
        {
            secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey no configurado en appsettings");
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
}
