using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Auth;

namespace SmartBookAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RegisterWithPhoneAsync(RegisterWithPhoneRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> GoogleAuthAsync(ExternalAuthRequest request);
    Task<ApiResponse<string>> VerifyPhoneAsync(VerifyPhoneRequest request);
    Task<ApiResponse<string>> ResendSmsCodeAsync(ResendSmsRequest request);
    Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
}
