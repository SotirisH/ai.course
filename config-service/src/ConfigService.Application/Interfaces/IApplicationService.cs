using ConfigService.Application.DTOs;

namespace ConfigService.Application.Interfaces;

public interface IApplicationService
{
    Task<ApplicationDto> CreateAsync(CreateApplicationRequest request);
    Task<ApplicationDto> UpdateAsync(string id, UpdateApplicationRequest request);
    Task<ApplicationDto> GetByIdAsync(string id);
    Task<IEnumerable<ApplicationDto>> GetAllAsync();
}

