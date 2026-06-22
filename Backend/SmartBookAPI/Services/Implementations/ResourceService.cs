using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Resource;
using SmartBookAPI.Models;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Interfaces;

namespace SmartBookAPI.Services.Implementations;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceService(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<ApiResponse<IEnumerable<ResourceResponse>>> GetAllAsync()
    {
        var resources = await _resourceRepository.GetAllAsync();
        var response = resources.Select(MapToResponse);
        return ApiResponse<IEnumerable<ResourceResponse>>.Ok(response, "Recursos obtenidos exitosamente");
    }

    public async Task<ApiResponse<ResourceResponse>> GetByIdAsync(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null)
        {
            return ApiResponse<ResourceResponse>.Fail("Recurso no encontrado");
        }

        return ApiResponse<ResourceResponse>.Ok(MapToResponse(resource));
    }

    public async Task<ApiResponse<ResourceResponse>> CreateAsync(ResourceRequest request)
    {
        var resource = new Resource
        {
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            IsActive = request.IsActive
        };

        await _resourceRepository.CreateAsync(resource);
        return ApiResponse<ResourceResponse>.Ok(MapToResponse(resource), "Recurso creado exitosamente");
    }

    public async Task<ApiResponse<ResourceResponse>> UpdateAsync(int id, ResourceRequest request)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null)
        {
            return ApiResponse<ResourceResponse>.Fail("Recurso no encontrado");
        }

        resource.Name = request.Name;
        resource.Description = request.Description;
        resource.Location = request.Location;
        resource.IsActive = request.IsActive;

        await _resourceRepository.UpdateAsync(resource);
        return ApiResponse<ResourceResponse>.Ok(MapToResponse(resource), "Recurso actualizado exitosamente");
    }

    public async Task<ApiResponse> DeleteAsync(int id)
    {
        if (!await _resourceRepository.ExistsAsync(id))
        {
            return ApiResponse.Fail("Recurso no encontrado");
        }

        try
        {
            await _resourceRepository.DeleteAsync(id);
            return ApiResponse.Ok("Recurso eliminado exitosamente");
        }
        catch
        {
            return ApiResponse.Fail("No se puede eliminar el recurso porque tiene reservas asociadas");
        }
    }

    private static ResourceResponse MapToResponse(Resource resource)
    {
        return new ResourceResponse
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Description = resource.Description,
            Location = resource.Location,
            IsActive = resource.IsActive
        };
    }
}
