using SmartBookAPI.DTOs;
using SmartBookAPI.DTOs.Resource;

namespace SmartBookAPI.Services.Interfaces;

public interface IResourceService
{
    Task<ApiResponse<IEnumerable<ResourceResponse>>> GetAllAsync();
    Task<ApiResponse<ResourceResponse>> GetByIdAsync(int id);
    Task<ApiResponse<ResourceResponse>> CreateAsync(ResourceRequest request);
    Task<ApiResponse<ResourceResponse>> UpdateAsync(int id, ResourceRequest request);
    Task<ApiResponse> DeleteAsync(int id);
}
