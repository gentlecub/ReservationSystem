using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.DTOs.Reservation;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Obtener reservas. Admin: todas | Client: solo las suyas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (IsAdmin())
        {
            var result = await _reservationService.GetAllAsync();
            return Ok(result);
        }
        else
        {
            var userId = GetCurrentUserId();
            var result = await _reservationService.GetByUserIdAsync(userId);
            return Ok(result);
        }
    }

    /// <summary>
    /// Obtener detalle de una reserva
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reservationService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        // Si no es admin, verificar que sea su reserva
        if (!IsAdmin() && result.Data?.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva reserva (Client)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _reservationService.CreateAsync(userId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.ReservationId }, result);
    }

    /// <summary>
    /// Actualizar estado de una reserva (solo Admin)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReservationUpdateRequest request)
    {
        var result = await _reservationService.UpdateStatusAsync(id, request);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Modificar una reserva (fecha, hora, recurso). Client: solo las suyas | Admin: cualquiera
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReservationRequest request)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        var result = await _reservationService.UpdateAsync(id, userId, isAdmin, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancelar una reserva. Client: solo las suyas | Admin: cualquiera
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        var result = await _reservationService.DeleteAsync(id, userId, isAdmin);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
}
