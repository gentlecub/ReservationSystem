using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Reservation;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;
using SmartBookAPI.DTOs.Waitlist;

namespace SmartBookAPI.Services.Implementations;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly INotificationService _notificationService;
    private readonly ICalendarService _calendarService;
    private readonly IWaitlistRepository _waitlistRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IResourceRepository resourceRepository,
        INotificationService notificationService,
        ICalendarService calendarService,
        IWaitlistRepository waitlistRepository)
    {
        _reservationRepository = reservationRepository;
        _resourceRepository = resourceRepository;
        _notificationService = notificationService;
        _calendarService = calendarService;
        _waitlistRepository = waitlistRepository;
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

        // Cargar el recurso para la notificación
        reservation.Resource = resource;

        // Enviar notificación de reserva creada
        await _notificationService.NotifyReservationCreatedAsync(reservation);

        // Crear evento en calendarios conectados
        await _calendarService.CreateEventAsync(reservation);

        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation), "Reserva creada exitosamente");
    }

    public async Task<ApiResponse<ReservationResponse>> UpdateStatusAsync(int id, ReservationUpdateRequest request)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return ApiResponse<ReservationResponse>.Fail("Reserva no encontrada");
        }

        var previousStatus = reservation.Status;
        reservation.Status = request.Status;
        await _reservationRepository.UpdateAsync(reservation);

        // Enviar notificaciones según el cambio de estado
        if (request.Status == "Confirmed" && previousStatus != "Confirmed")
        {
            await _notificationService.NotifyReservationConfirmedAsync(reservation);
            // Crear evento en calendario al confirmar
            await _calendarService.CreateEventAsync(reservation);
        }
        else if (request.Status == "Cancelled" && previousStatus != "Cancelled")
        {
            await _notificationService.NotifyReservationCancelledAsync(reservation, "admin");
            // Eliminar evento del calendario al cancelar
            await _calendarService.DeleteEventAsync(reservation);
            // Procesar lista de espera
            await ProcessWaitlistOnCancellationAsync(reservation);
        }

        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation), "Estado de reserva actualizado");
    }

    public async Task<ApiResponse<ReservationResponse>> UpdateAsync(int id, int userId, bool isAdmin, ReservationRequest request)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return ApiResponse<ReservationResponse>.Fail("Reserva no encontrada");
        }

        // Si no es admin, solo puede modificar sus propias reservas
        if (!isAdmin && reservation.UserId != userId)
        {
            return ApiResponse<ReservationResponse>.Fail("No tienes permiso para modificar esta reserva");
        }

        // Solo se pueden modificar reservas pendientes o confirmadas
        if (reservation.Status == "Cancelled")
        {
            return ApiResponse<ReservationResponse>.Fail("No se puede modificar una reserva cancelada");
        }

        // Validar que el recurso existe y está activo si se cambia
        if (request.ResourceId != reservation.ResourceId)
        {
            var resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
            {
                return ApiResponse<ReservationResponse>.Fail("Recurso no encontrado");
            }
            if (!resource.IsActive)
            {
                return ApiResponse<ReservationResponse>.Fail("El recurso no está disponible");
            }
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

        // Verificar disponibilidad (excluyendo la reserva actual)
        var hasConflict = await _reservationRepository.HasConflictAsync(
            request.ResourceId,
            request.Date,
            request.StartTime,
            request.EndTime,
            id); // Excluir la reserva actual

        if (hasConflict)
        {
            return ApiResponse<ReservationResponse>.Fail(
                "El recurso ya está reservado en ese horario. Por favor seleccione otro horario.");
        }

        // Construir descripción de cambios para la notificación
        var changes = new List<string>();
        if (reservation.ResourceId != request.ResourceId)
        {
            var newResource = await _resourceRepository.GetByIdAsync(request.ResourceId);
            changes.Add($"Recurso: {reservation.Resource?.Name ?? "anterior"} → {newResource?.Name ?? "nuevo"}");
        }
        if (reservation.Date != request.Date)
        {
            changes.Add($"Fecha: {reservation.Date:dd/MM/yyyy} → {request.Date:dd/MM/yyyy}");
        }
        if (reservation.StartTime != request.StartTime || reservation.EndTime != request.EndTime)
        {
            changes.Add($"Horario: {reservation.StartTime:HH:mm}-{reservation.EndTime:HH:mm} → {request.StartTime:HH:mm}-{request.EndTime:HH:mm}");
        }

        // Actualizar la reserva
        reservation.ResourceId = request.ResourceId;
        reservation.Date = request.Date;
        reservation.StartTime = request.StartTime;
        reservation.EndTime = request.EndTime;
        reservation.ReminderSent = false; // Resetear recordatorio si cambia la fecha

        await _reservationRepository.UpdateAsync(reservation);

        // Recargar con el recurso actualizado
        reservation = await _reservationRepository.GetByIdAsync(id);

        // Notificar al usuario sobre la modificación
        if (changes.Count > 0)
        {
            var changesText = string.Join(", ", changes);
            await _notificationService.NotifyReservationModifiedAsync(reservation!, changesText);
            // Actualizar evento en calendarios conectados
            await _calendarService.UpdateEventAsync(reservation!);
        }

        return ApiResponse<ReservationResponse>.Ok(MapToResponse(reservation!), "Reserva modificada exitosamente");
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

        var previousStatus = reservation.Status;

        // Cambiar estado a Cancelled en lugar de eliminar físicamente
        reservation.Status = "Cancelled";
        await _reservationRepository.UpdateAsync(reservation);

        // Enviar notificación de cancelación si no estaba ya cancelada
        if (previousStatus != "Cancelled")
        {
            var cancelledBy = isAdmin ? "admin" : "user";
            await _notificationService.NotifyReservationCancelledAsync(reservation, cancelledBy);
            // Eliminar evento del calendario
            await _calendarService.DeleteEventAsync(reservation);

            // Procesar lista de espera: notificar al primer usuario en la cola
            await ProcessWaitlistOnCancellationAsync(reservation);
        }

        return ApiResponse.Ok("Reserva cancelada exitosamente");
    }

    /// <summary>
    /// Procesa la lista de espera cuando se cancela una reserva
    /// </summary>
    private async Task ProcessWaitlistOnCancellationAsync(Reservation cancelledReservation)
    {
        try
        {
            var firstInQueue = await _waitlistRepository.GetFirstInQueueAsync(
                cancelledReservation.ResourceId,
                cancelledReservation.Date);

            if (firstInQueue != null)
            {
                // Marcar como notificado
                firstInQueue.Status = "Notified";
                firstInQueue.NotifiedAt = DateTime.UtcNow;
                await _waitlistRepository.UpdateAsync(firstInQueue);

                // Aquí podríamos enviar notificación al usuario de la lista de espera
                // indicando que hay disponibilidad
            }
        }
        catch
        {
            // No bloquear la cancelación si falla el procesamiento de lista de espera
        }
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
