using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Profile;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;

    public ProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<ProfileResponse>> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<ProfileResponse>.Fail("Usuario no encontrado");
        }

        var profile = new ProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            PhoneVerified = user.PhoneVerified,
            AuthProvider = user.AuthProvider,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role?.RoleName ?? "Client",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return ApiResponse<ProfileResponse>.Ok(profile);
    }

    public async Task<ApiResponse<ProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<ProfileResponse>.Fail("Usuario no encontrado");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
        {
            // Verificar que el teléfono no esté en uso por otro usuario
            if (await _userRepository.PhoneExistsAsync(request.PhoneNumber))
            {
                return ApiResponse<ProfileResponse>.Fail("El número de teléfono ya está en uso");
            }
            user.PhoneNumber = request.PhoneNumber;
            user.PhoneVerified = false;
        }

        if (request.ProfilePhotoUrl != null)
        {
            user.ProfilePhotoUrl = request.ProfilePhotoUrl;
        }

        await _userRepository.UpdateAsync(user);

        var profile = new ProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            PhoneVerified = user.PhoneVerified,
            AuthProvider = user.AuthProvider,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role?.RoleName ?? "Client",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return ApiResponse<ProfileResponse>.Ok(profile, "Perfil actualizado exitosamente");
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Usuario no encontrado");
        }

        if (user.AuthProvider != "Local" && user.AuthProvider != "Phone")
        {
            return ApiResponse<string>.Fail($"No puedes cambiar la contraseña. Tu cuenta usa {user.AuthProvider}");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return ApiResponse<string>.Fail("Esta cuenta no tiene contraseña configurada");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<string>.Fail("La contraseña actual es incorrecta");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.Ok("Contraseña cambiada exitosamente");
    }

    public async Task<ApiResponse<string>> DeleteAccountAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<string>.Fail("Usuario no encontrado");
        }

        // Soft delete - desactivar la cuenta
        user.IsActive = false;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.Ok("Cuenta eliminada exitosamente");
    }
}
