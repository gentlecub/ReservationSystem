using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.DTOs.Auth;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registrar un nuevo usuario (rol Client por defecto)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Iniciar sesión y obtener JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}
