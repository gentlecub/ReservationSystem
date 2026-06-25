using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.DTOs.Waitlist;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaitlistController : ControllerBase
{
    private readonly IWaitlistService _waitlistService;

    public WaitlistController(IWaitlistService waitlistService)
    {
        _waitlistService = waitlistService;
    }

    /// <summary>
    /// Obtener lista de espera. Admin: todas | Client: solo las suyas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (IsAdmin())
        {
            var result = await _waitlistService.GetAllAsync();
            return Ok(result);
        }
        else
        {
            var userId = GetCurrentUserId();
            var result = await _waitlistService.GetByUserIdAsync(userId);
            return Ok(result);
        }
    }

    /// <summary>
    /// Obtener lista de espera por recurso (solo Admin)
    /// </summary>
    [HttpGet("resource/{resourceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByResource(int resourceId)
    {
        var result = await _waitlistService.GetByResourceIdAsync(resourceId);
        return Ok(result);
    }

    /// <summary>
    /// Obtener detalle de una entrada en lista de espera
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _waitlistService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        // Si no es admin, verificar que sea su entrada
        if (!IsAdmin() && result.Data?.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtener posicion en la cola
    /// </summary>
    [HttpGet("{id}/position")]
    public async Task<IActionResult> GetPosition(int id)
    {
        // Primero verificar que el usuario tenga acceso a esta entrada
        var entry = await _waitlistService.GetByIdAsync(id);
        if (!entry.Success)
            return NotFound(entry);

        if (!IsAdmin() && entry.Data?.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        var result = await _waitlistService.GetPositionAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Agregar a lista de espera
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToWaitlist([FromBody] WaitlistRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _waitlistService.AddToWaitlistAsync(userId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.WaitlistId }, result);
    }

    /// <summary>
    /// Cancelar entrada en lista de espera (usuario)
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _waitlistService.CancelByUserAsync(id, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Eliminar de lista de espera. Client: solo las suyas | Admin: cualquiera
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        var result = await _waitlistService.RemoveFromWaitlistAsync(id, userId, isAdmin);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Notificar al siguiente en la cola (solo Admin)
    /// </summary>
    [HttpPost("notify/{resourceId}/{date}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyNext(int resourceId, DateOnly date)
    {
        var result = await _waitlistService.NotifyNextInQueueAsync(resourceId, date);
        return Ok(result);
    }

    /// <summary>
    /// Expirar entradas antiguas (solo Admin)
    /// </summary>
    [HttpPost("expire")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExpireOldEntries()
    {
        var result = await _waitlistService.ExpireOldEntriesAsync();
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
