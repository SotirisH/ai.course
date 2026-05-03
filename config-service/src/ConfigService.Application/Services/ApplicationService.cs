using ConfigService.Application.DTOs;
using ConfigService.Application.Interfaces;
using ConfigService.Domain.Entities;
using ConfigService.Domain.Exceptions;

namespace ConfigService.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repository;
    
    public ApplicationService(IApplicationRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ApplicationDto> CreateAsync(CreateApplicationRequest request)
    {
        // Check if application with same name exists
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "An application with this name already exists." } }
            });
        }
        
        var application = new Domain.Entities.Application
        {
            Name = request.Name,
            Comments = request.Comments
        };
        
        var created = await _repository.CreateAsync(application);
        
        return MapToDto(created);
    }
    
    public async Task<ApplicationDto> UpdateAsync(string id, UpdateApplicationRequest request)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), id);
        }
        
        // Check if another application with same name exists
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null && existing.Id != id)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "An application with this name already exists." } }
            });
        }
        
        application.Name = request.Name;
        application.Comments = request.Comments;
        
        var updated = await _repository.UpdateAsync(application);
        
        return MapToDto(updated);
    }
    
    public async Task<ApplicationDto> GetByIdAsync(string id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), id);
        }
        
        return MapToDto(application);
    }
    
    public async Task<IEnumerable<ApplicationDto>> GetAllAsync()
    {
        var applications = await _repository.GetAllAsync();
        return applications.Select(MapToDto);
    }
    
    private static ApplicationDto MapToDto(Domain.Entities.Application application)
    {
        return new ApplicationDto
        {
            Id = application.Id,
            Name = application.Name,
            Comments = application.Comments,
            ConfigurationIds = application.Configurations.Select(c => c.Id).ToList()
        };
    }
}
