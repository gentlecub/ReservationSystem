using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Obtener todos los usuarios (solo Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Eliminar un usuario (solo Admin)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
