using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.User;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var response = users.Select(MapToResponse);
        return ApiResponse<IEnumerable<UserResponse>>.Ok(response, "Usuarios obtenidos exitosamente");
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("Usuario no encontrado");
        }

        return ApiResponse<UserResponse>.Ok(MapToResponse(user));
    }

    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ApiResponse.Fail("Usuario no encontrado");
        }

        // No permitir eliminar administradores
        if (user.Role?.RoleName == "Admin")
        {
            return ApiResponse.Fail("No se puede eliminar un administrador");
        }

        await _userRepository.DeleteAsync(id);
        return ApiResponse.Ok("Usuario eliminado exitosamente");
    }

    private static UserResponse MapToResponse(Models.User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Client",
            CreatedAt = user.CreatedAt
        };
    }
}
