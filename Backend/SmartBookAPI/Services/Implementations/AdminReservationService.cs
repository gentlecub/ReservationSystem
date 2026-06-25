using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Data;
using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Reservation;
using SmartBookAPI.Models;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class AdminReservationService : IAdminReservationService
{
    private readonly AppDbContext _context;
    private readonly IWaitlistService _waitlistService;

    public AdminReservationService(AppDbContext context, IWaitlistService waitlistService)
    {
        _context = context;
        _waitlistService = waitlistService;
    }

    public async Task<ApiResponse<AdminReservationListResponse>> GetWithFiltersAsync(AdminReservationFilterRequest filter)
    {
        var query = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .AsQueryable();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Aplicar filtros
        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(r => r.Status == filter.Status);
        }

        if (filter.ResourceId.HasValue)
        {
            query = query.Where(r => r.ResourceId == filter.ResourceId.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(r => r.UserId == filter.UserId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(r => r.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(r => r.Date <= filter.ToDate.Value);
        }

        if (filter.ActiveOnly == true)
        {
            query = query.Where(r => (r.Status == "Pending" || r.Status == "Confirmed") && r.Date >= today);
        }

        if (filter.HistoryOnly == true)
        {
            query = query.Where(r => r.Status == "Cancelled" || r.Date < today);
        }

        // Contar total antes de paginar
        var totalCount = await query.CountAsync();

        // Aplicar ordenamiento
        query = filter.SortBy?.ToLower() switch
        {
            "createdat" => filter.SortDescending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            "status" => filter.SortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "resourcename" => filter.SortDescending
                ? query.OrderByDescending(r => r.Resource!.Name)
                : query.OrderBy(r => r.Resource!.Name),
            "username" => filter.SortDescending
                ? query.OrderByDescending(r => r.User!.FullName)
                : query.OrderBy(r => r.User!.FullName),
            _ => filter.SortDescending
                ? query.OrderByDescending(r => r.Date).ThenByDescending(r => r.StartTime)
                : query.OrderBy(r => r.Date).ThenBy(r => r.StartTime)
        };

        // Aplicar paginacion
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var reservations = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        // Calcular resumen
        var summary = await GetSummaryInternalAsync();

        var response = new AdminReservationListResponse
        {
            Reservations = reservations.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Summary = summary
        };

        return ApiResponse<AdminReservationListResponse>.Ok(response, "Reservas obtenidas exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<ReservationResponse>>> GetActiveReservationsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var reservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .Where(r => (r.Status == "Pending" || r.Status == "Confirmed") && r.Date >= today)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();

        var response = reservations.Select(MapToResponse);
        return ApiResponse<IEnumerable<ReservationResponse>>.Ok(response, "Reservas activas obtenidas exitosamente");
    }

    public async Task<ApiResponse<IEnumerable<ReservationResponse>>> GetReservationHistoryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var reservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Resource)
            .Where(r => r.Status == "Cancelled" || r.Date < today)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToListAsync();

        var response = reservations.Select(MapToResponse);
        return ApiResponse<IEnumerable<ReservationResponse>>.Ok(response, "Historial de reservas obtenido exitosamente");
    }

    public async Task<ApiResponse<int>> BulkUpdateStatusAsync(List<int> reservationIds, string newStatus)
    {
        if (!new[] { "Pending", "Confirmed", "Cancelled" }.Contains(newStatus))
        {
            return ApiResponse<int>.Fail("Estado no válido. Use: Pending, Confirmed o Cancelled");
        }

        var reservations = await _context.Reservations
            .Include(r => r.Resource)
            .Where(r => reservationIds.Contains(r.ReservationId))
            .ToListAsync();

        var updatedCount = 0;
        foreach (var reservation in reservations)
        {
            if (reservation.Status != newStatus)
            {
                var previousStatus = reservation.Status;
                reservation.Status = newStatus;
                updatedCount++;

                // Si se cancela, procesar lista de espera
                if (newStatus == "Cancelled" && previousStatus != "Cancelled")
                {
                    await _waitlistService.ProcessWaitlistForSlotAsync(
                        reservation.ResourceId,
                        reservation.Date,
                        reservation.StartTime,
                        reservation.EndTime);
                }
            }
        }

        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(updatedCount, $"{updatedCount} reservas actualizadas exitosamente");
    }

    public async Task<ApiResponse<ReservationSummary>> GetSummaryAsync()
    {
        var summary = await GetSummaryInternalAsync();
        return ApiResponse<ReservationSummary>.Ok(summary, "Resumen obtenido exitosamente");
    }

    private async Task<ReservationSummary> GetSummaryInternalAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var stats = await _context.Reservations
            .GroupBy(r => 1)
            .Select(g => new
            {
                TotalPending = g.Count(r => r.Status == "Pending"),
                TotalConfirmed = g.Count(r => r.Status == "Confirmed"),
                TotalCancelled = g.Count(r => r.Status == "Cancelled"),
                TotalActive = g.Count(r => (r.Status == "Pending" || r.Status == "Confirmed") && r.Date >= today),
                TotalHistory = g.Count(r => r.Status == "Cancelled" || r.Date < today)
            })
            .FirstOrDefaultAsync();

        return new ReservationSummary
        {
            TotalPending = stats?.TotalPending ?? 0,
            TotalConfirmed = stats?.TotalConfirmed ?? 0,
            TotalCancelled = stats?.TotalCancelled ?? 0,
            TotalActive = stats?.TotalActive ?? 0,
            TotalHistory = stats?.TotalHistory ?? 0
        };
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
