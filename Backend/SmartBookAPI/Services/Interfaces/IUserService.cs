using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.User;

namespace SmartBookAPI.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<IEnumerable<UserResponse>>> GetAllAsync();
    Task<ApiResponse<UserResponse>> GetByIdAsync(int id);
    Task<ApiResponse> DeleteAsync(int id);
}
