using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Reservation;

namespace SmartBookAPI.Services.Interfaces;

public interface IReservationService
{
    Task<ApiResponse<IEnumerable<ReservationResponse>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<ReservationResponse>>> GetByUserIdAsync(int userId);
    Task<ApiResponse<ReservationResponse>> GetByIdAsync(int id);
    Task<ApiResponse<ReservationResponse>> CreateAsync(int userId, ReservationRequest request);
    Task<ApiResponse<ReservationResponse>> UpdateStatusAsync(int id, ReservationUpdateRequest request);
    Task<ApiResponse<ReservationResponse>> UpdateAsync(int id, int userId, bool isAdmin, ReservationRequest request);
    Task<ApiResponse> DeleteAsync(int id, int userId, bool isAdmin);
}
