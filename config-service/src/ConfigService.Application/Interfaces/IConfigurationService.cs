using ConfigService.Application.DTOs;

namespace ConfigService.Application.Interfaces;

public interface IConfigurationService
{
    Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request);
    Task<ConfigurationDto> UpdateAsync(string id, UpdateConfigurationRequest request);
    Task<ConfigurationDto> GetByIdAsync(string id);
}

