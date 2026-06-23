using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Profile;

namespace SmartBookAPI.Services.Interfaces;

public interface IProfileService
{
    Task<ApiResponse<ProfileResponse>> GetProfileAsync(int userId);
    Task<ApiResponse<ProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ApiResponse<string>> DeleteAccountAsync(int userId);
}
