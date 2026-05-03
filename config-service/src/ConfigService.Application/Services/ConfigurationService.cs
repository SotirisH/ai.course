using ConfigService.Application.DTOs;
using ConfigService.Application.Interfaces;
using ConfigService.Domain.Entities;
using ConfigService.Domain.Exceptions;

namespace ConfigService.Application.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configRepository;
    private readonly IApplicationRepository _appRepository;
    
    public ConfigurationService(
        IConfigurationRepository configRepository,
        IApplicationRepository appRepository)
    {
        _configRepository = configRepository;
        _appRepository = appRepository;
    }
    
    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request)
    {
        // Verify application exists
        var application = await _appRepository.GetByIdAsync(request.ApplicationId);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), request.ApplicationId);
        }
        
        // Check if configuration with same name exists for this application
        var existing = await _configRepository.GetByApplicationAndNameAsync(
            request.ApplicationId, request.Name);
        if (existing != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "A configuration with this name already exists for this application." } }
            });
        }
        
        var configuration = new Configuration
        {
            ApplicationId = request.ApplicationId,
            Name = request.Name,
            Comments = request.Comments,
            Config = request.Config
        };
        
        var created = await _configRepository.CreateAsync(configuration);
        
        return MapToDto(created);
    }
    
    public async Task<ConfigurationDto> UpdateAsync(string id, UpdateConfigurationRequest request)
    {
        var configuration = await _configRepository.GetByIdAsync(id);
        if (configuration == null)
        {
            throw new NotFoundException(nameof(Configuration), id);
        }
        
        // Check if another configuration with same name exists for this application
        var existing = await _configRepository.GetByApplicationAndNameAsync(
            configuration.ApplicationId, request.Name);
        if (existing != null && existing.Id != id)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "A configuration with this name already exists for this application." } }
            });
        }
        
        configuration.Name = request.Name;
        configuration.Comments = request.Comments;
        configuration.Config = request.Config;
        
        var updated = await _configRepository.UpdateAsync(configuration);
        
        return MapToDto(updated);
    }
    
    public async Task<ConfigurationDto> GetByIdAsync(string id)
    {
        var configuration = await _configRepository.GetByIdAsync(id);
        if (configuration == null)
        {
            throw new NotFoundException(nameof(Configuration), id);
        }
        
        return MapToDto(configuration);
    }
    
    private static ConfigurationDto MapToDto(Configuration configuration)
    {
        return new ConfigurationDto
        {
            Id = configuration.Id,
            ApplicationId = configuration.ApplicationId,
            Name = configuration.Name,
            Comments = configuration.Comments,
            Config = configuration.Config
        };
    }
}
