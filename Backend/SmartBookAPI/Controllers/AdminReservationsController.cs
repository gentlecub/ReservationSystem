using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.DTOs.Reservation;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/admin/reservations")]
[Authorize(Roles = "Admin")]
public class AdminReservationsController : ControllerBase
{
    private readonly IAdminReservationService _adminReservationService;
    private readonly IReservationService _reservationService;

    public AdminReservationsController(
        IAdminReservationService adminReservationService,
        IReservationService reservationService)
    {
        _adminReservationService = adminReservationService;
        _reservationService = reservationService;
    }

    /// <summary>
    /// Obtener reservas con filtros avanzados y paginacion
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWithFilters([FromQuery] AdminReservationFilterRequest filter)
    {
        var result = await _adminReservationService.GetWithFiltersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Obtener solo reservas activas (pendientes o confirmadas, no pasadas)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _adminReservationService.GetActiveReservationsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtener historial de reservas (pasadas o canceladas)
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _adminReservationService.GetReservationHistoryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtener resumen estadistico de reservas
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _adminReservationService.GetSummaryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtener detalle de una reserva especifica
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reservationService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Modificar una reserva (fecha, hora, recurso)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReservationRequest request)
    {
        var result = await _reservationService.UpdateAsync(id, 0, true, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualizar estado de una reserva
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReservationUpdateRequest request)
    {
        var result = await _reservationService.UpdateStatusAsync(id, request);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancelar una reserva
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _reservationService.DeleteAsync(id, 0, true);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualizar estado de multiples reservas a la vez
    /// </summary>
    [HttpPut("bulk-status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusUpdateRequest request)
    {
        if (request.ReservationIds == null || request.ReservationIds.Count == 0)
        {
            return BadRequest(new { Success = false, Message = "Debe proporcionar al menos un ID de reserva" });
        }

        var result = await _adminReservationService.BulkUpdateStatusAsync(request.ReservationIds, request.Status);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

/// <summary>
/// DTO para actualizacion masiva de estado
/// </summary>
public class BulkStatusUpdateRequest
{
    public List<int> ReservationIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}
