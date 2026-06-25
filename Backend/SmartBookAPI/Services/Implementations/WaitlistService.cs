using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Waitlist;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class WaitlistService : IWaitlistService
{
    private readonly IWaitlistRepository _waitlistRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly INotificationService _notificationService;

    public WaitlistService(
        IWaitlistRepository waitlistRepository,
        IResourceRepository resourceRepository,
        INotificationService notificationService)
    {
        _waitlistRepository = waitlistRepository;
        _resourceRepository = resourceRepository;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetAllAsync()
    {
        var entries = await _waitlistRepository.GetAllAsync();
        var response = entries.Select(MapToResponse);
        return ApiResponse<IEnumerable<WaitlistResponse>>.Ok(response, "Lista de espera obtenida exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetByUserIdAsync(int userId)
    {
        var entries = await _waitlistRepository.GetByUserIdAsync(userId);
        var response = entries.Select(MapToResponse);
        return ApiResponse<IEnumerable<WaitlistResponse>>.Ok(response, "Lista de espera del usuario obtenida exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<WaitlistResponse>>> GetByResourceIdAsync(int resourceId)
    {
        var entries = await _waitlistRepository.GetByResourceIdAsync(resourceId);
        var response = entries.Select(MapToResponse);
        return ApiResponse<IEnumerable<WaitlistResponse>>.Ok(response, "Lista de espera del recurso obtenida exitosamente");
    }

    public async Task<ApiResponse<WaitlistResponse>> GetByIdAsync(int id)
    {
        var entry = await _waitlistRepository.GetByIdAsync(id);
        if (entry == null)
        {
            return ApiResponse<WaitlistResponse>.Fail("Entrada en lista de espera no encontrada");
        }

        return ApiResponse<WaitlistResponse>.Ok(MapToResponse(entry));
    }

    public async Task<ApiResponse<WaitlistResponse>> AddToWaitlistAsync(int userId, WaitlistRequest request)
    {
        // Validar que el recurso existe y está activo
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId);
        if (resource == null)
        {
            return ApiResponse<WaitlistResponse>.Fail("Recurso no encontrado");
        }

        if (!resource.IsActive)
        {
            return ApiResponse<WaitlistResponse>.Fail("El recurso no está disponible");
        }

        // Validar horarios si se proporcionan
        if (request.PreferredStartTime.HasValue && request.PreferredEndTime.HasValue)
        {
            if (request.PreferredEndTime <= request.PreferredStartTime)
            {
                return ApiResponse<WaitlistResponse>.Fail("La hora de fin debe ser mayor a la hora de inicio");
            }
        }

        // Validar que la fecha no sea en el pasado
        if (request.PreferredDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return ApiResponse<WaitlistResponse>.Fail("No se puede agregar a lista de espera para fechas pasadas");
        }

        // Verificar si ya existe una entrada activa para el mismo usuario, recurso y fecha
        var hasActiveEntry = await _waitlistRepository.HasActiveEntryAsync(userId, request.ResourceId, request.PreferredDate);
        if (hasActiveEntry)
        {
            return ApiResponse<WaitlistResponse>.Fail("Ya estás en la lista de espera para este recurso y fecha");
        }

        // Obtener la siguiente posicion en la cola
        var position = await _waitlistRepository.GetNextPositionAsync(request.ResourceId, request.PreferredDate);

        // Crear la entrada en lista de espera
        var entry = new WaitlistEntry
        {
            UserId = userId,
            ResourceId = request.ResourceId,
            PreferredDate = request.PreferredDate,
            PreferredStartTime = request.PreferredStartTime,
            PreferredEndTime = request.PreferredEndTime,
            Position = position,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Expira en 7 días por defecto
        };

        await _waitlistRepository.CreateAsync(entry);

        return ApiResponse<WaitlistResponse>.Ok(MapToResponse(entry),
            $"Agregado a la lista de espera en posición {position}");
    }

    public async Task<ApiResponse> RemoveFromWaitlistAsync(int id, int userId, bool isAdmin)
    {
        var entry = await _waitlistRepository.GetByIdAsync(id);
        if (entry == null)
        {
            return ApiResponse.Fail("Entrada en lista de espera no encontrada");
        }

        // Si no es admin, solo puede eliminar sus propias entradas
        if (!isAdmin && entry.UserId != userId)
        {
            return ApiResponse.Fail("No tienes permiso para eliminar esta entrada");
        }

        var resourceId = entry.ResourceId;
        var date = entry.PreferredDate;

        await _waitlistRepository.DeleteAsync(id);

        // Reordenar posiciones
        await _waitlistRepository.ReorderPositionsAsync(resourceId, date);

        return ApiResponse.Ok("Eliminado de la lista de espera exitosamente");
    }

    public async Task<ApiResponse> CancelByUserAsync(int id, int userId)
    {
        var entry = await _waitlistRepository.GetByIdAsync(id);
        if (entry == null)
        {
            return ApiResponse.Fail("Entrada en lista de espera no encontrada");
        }

        if (entry.UserId != userId)
        {
            return ApiResponse.Fail("No tienes permiso para cancelar esta entrada");
        }

        entry.Status = "Cancelled";
        await _waitlistRepository.UpdateAsync(entry);

        // Reordenar posiciones
        await _waitlistRepository.ReorderPositionsAsync(entry.ResourceId, entry.PreferredDate);

        return ApiResponse.Ok("Entrada en lista de espera cancelada exitosamente");
    }

    public async Task<ApiResponse<WaitlistResponse?>> ProcessWaitlistForSlotAsync(int resourceId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        // Buscar el primer usuario en la cola para esta fecha y recurso
        var firstInQueue = await _waitlistRepository.GetFirstInQueueAsync(resourceId, date);

        if (firstInQueue == null)
        {
            return ApiResponse<WaitlistResponse?>.Ok(null, "No hay usuarios en lista de espera");
        }

        // Si el usuario tiene preferencia de horario, verificar si coincide
        if (firstInQueue.PreferredStartTime.HasValue && firstInQueue.PreferredEndTime.HasValue)
        {
            // Verificar si el slot liberado coincide con la preferencia
            var slotMatches = startTime >= firstInQueue.PreferredStartTime.Value &&
                              endTime <= firstInQueue.PreferredEndTime.Value;

            if (!slotMatches)
            {
                // Notificar pero no marcar como cumplida
                firstInQueue.Status = "Notified";
                firstInQueue.NotifiedAt = DateTime.UtcNow;
                await _waitlistRepository.UpdateAsync(firstInQueue);

                return ApiResponse<WaitlistResponse?>.Ok(MapToResponse(firstInQueue),
                    "Usuario notificado de disponibilidad parcial");
            }
        }

        // Marcar como notificado
        firstInQueue.Status = "Notified";
        firstInQueue.NotifiedAt = DateTime.UtcNow;
        await _waitlistRepository.UpdateAsync(firstInQueue);

        // Aquí podrías enviar una notificación al usuario
        // _ = _notificationService.NotifyWaitlistAvailabilityAsync(firstInQueue);

        return ApiResponse<WaitlistResponse?>.Ok(MapToResponse(firstInQueue),
            "Usuario en lista de espera notificado");
    }

    public async Task<ApiResponse> NotifyNextInQueueAsync(int resourceId, DateOnly date)
    {
        var firstInQueue = await _waitlistRepository.GetFirstInQueueAsync(resourceId, date);

        if (firstInQueue == null)
        {
            return ApiResponse.Ok("No hay usuarios en lista de espera para notificar");
        }

        firstInQueue.Status = "Notified";
        firstInQueue.NotifiedAt = DateTime.UtcNow;
        await _waitlistRepository.UpdateAsync(firstInQueue);

        return ApiResponse.Ok($"Usuario {firstInQueue.User?.FullName} notificado de disponibilidad");
    }

    public async Task<ApiResponse<int>> ExpireOldEntriesAsync()
    {
        var expiredEntries = await _waitlistRepository.GetExpiredEntriesAsync();
        var count = 0;

        foreach (var entry in expiredEntries)
        {
            entry.Status = "Expired";
            await _waitlistRepository.UpdateAsync(entry);
            count++;
        }

        return ApiResponse<int>.Ok(count, $"{count} entradas marcadas como expiradas");
    }

    public async Task<ApiResponse<int>> GetPositionAsync(int waitlistId)
    {
        var entry = await _waitlistRepository.GetByIdAsync(waitlistId);
        if (entry == null)
        {
            return ApiResponse<int>.Fail("Entrada en lista de espera no encontrada");
        }

        return ApiResponse<int>.Ok(entry.Position, $"Posición actual: {entry.Position}");
    }

    private static WaitlistResponse MapToResponse(WaitlistEntry entry)
    {
        return new WaitlistResponse
        {
            WaitlistId = entry.WaitlistId,
            UserId = entry.UserId,
            UserName = entry.User?.FullName ?? "",
            UserEmail = entry.User?.Email ?? "",
            ResourceId = entry.ResourceId,
            ResourceName = entry.Resource?.Name ?? "",
            ResourceLocation = entry.Resource?.Location,
            PreferredDate = entry.PreferredDate,
            PreferredStartTime = entry.PreferredStartTime,
            PreferredEndTime = entry.PreferredEndTime,
            Position = entry.Position,
            Status = entry.Status,
            CreatedAt = entry.CreatedAt,
            ExpiresAt = entry.ExpiresAt,
            NotifiedAt = entry.NotifiedAt
        };
    }
}
