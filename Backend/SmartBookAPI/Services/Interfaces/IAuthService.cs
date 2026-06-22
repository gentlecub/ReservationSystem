using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Auth;

namespace SmartBookAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
}
