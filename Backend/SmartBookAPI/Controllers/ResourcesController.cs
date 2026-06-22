using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.DTOs.Resource;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    /// <summary>
    /// Obtener todos los recursos (público)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _resourceService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtener un recurso por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _resourceService.GetByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo recurso (solo Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ResourceRequest request)
    {
        var result = await _resourceService.CreateAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.ResourceId }, result);
    }

    /// <summary>
    /// Actualizar un recurso (solo Admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ResourceRequest request)
    {
        var result = await _resourceService.UpdateAsync(id, request);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Eliminar un recurso (solo Admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _resourceService.DeleteAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
