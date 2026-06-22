using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Reservation;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IResourceRepository _resourceRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IResourceRepository resourceRepository)
    {
        _reservationRepository = reservationRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<ApiResponse<IEnumerable<ReservationResponse>>> GetAllAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        var response = reservations.Select(MapToResponse);
        return ApiResponse<IEnumerable<ReservationResponse>>.Ok(response, "Reservas obtenidas exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<ReservationResponse>>> GetByUserIdAsync(int userId)
    {
        var reservations = await _reservationRepository.GetByUserIdAsync(userId);
        var response = reservations.Select(MapToResponse);
        return ApiResponse<IEnumerable<ReservationResponse>>.Ok(response, "Reservas obtenidas exitosamente");
    }

    public async Task<ApiResponse<ReservationResponse>> GetByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return ApiResponse<ReservationResponse>.Fail("Reserva no encontrada");
        }

        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation));
    }

    public async Task<ApiResponse<ReservationResponse>> CreateAsync(int userId, ReservationRequest request)
    {
        // Validar que el recurso existe y está activo
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
        if (resource == null)
        {
            return ApiResponse<ReservationResponse>.Fail("Recurso no encontrado");
        }

        if (!resource.IsActive)
        {
            return ApiResponse<ReservationResponse>.Fail("El recurso no está disponible");
        }

        // Validar que la hora de fin sea mayor a la de inicio
        if (request.EndTime <= request.StartTime)
        {
            return ApiResponse<ReservationResponse>.Fail("La hora de fin debe ser mayor a la hora de inicio");
        }

        // Validar que la fecha no sea en el pasado
        if (request.Date < DateOnly.FromDateTime(DateTime.Today))
        {
            return ApiResponse<ReservationResponse>.Fail("No se puede reservar en fechas pasadas");
        }

        // Verificar disponibilidad (no debe haber conflicto de horarios)
        var hasConflict = await _reservationRepository.HasConflictAsync(
            request.ResourceId,
            request.Date,
            request.StartTime,
            request.EndTime);

        if (hasConflict)
        {
            return ApiResponse<ReservationResponse>.Fail(
                "El recurso ya está reservado en ese horario. Por favor seleccione otro horario.");
        }

        // Crear la reserva
        var reservation = new Reservation
        {
            UserId = userId,
            ResourceId = request.ResourceId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.CreateAsync(reservation);
        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation), "Reserva creada exitosamente");
    }

    public async Task<ApiResponse<ReservationResponse>> UpdateStatusAsync(int id, ReservationUpdateRequest request)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return ApiResponse<ReservationResponse>.Fail("Reserva no encontrada");
        }

        reservation.Status = request.Status;
        await _reservationRepository.UpdateAsync(reservation);

        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation), "Estado de reserva actualizado");
    }

    public async Task<ApiResponse> DeleteAsync(int id, int userId, bool isAdmin)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return ApiResponse.Fail("Reserva no encontrada");
        }

        // Si no es admin, solo puede cancelar sus propias reservas
        if (!isAdmin && reservation.UserId != userId)
        {
            return ApiResponse.Fail("No tienes permiso para cancelar esta reserva");
        }

        // Cambiar estado a Cancelled en lugar de eliminar físicamente
        reservation.Status = "Cancelled";
        await _reservationRepository.UpdateAsync(reservation);

        return ApiResponse.Ok("Reserva cancelada exitosamente");
    }

    private static ReservationResponse MapToResponse(Reservation reservation)
    {
        return new ReservationResponse
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            UserName = reservation.User?.FullName ?? "",
            UserEmail = reservation.User?.Email ?? "",
            ResourceId = reservation.ResourceId,
            ResourceName = reservation.Resource?.Name ?? "",
            ResourceLocation = reservation.Resource?.Location,
            Date = reservation.Date,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt
        };
    }
}
