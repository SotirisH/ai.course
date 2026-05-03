using ConfigService.Domain.Entities;

namespace ConfigService.Application.Interfaces;

public interface IConfigurationRepository
{
    Task<Configuration?> GetByIdAsync(string id);
    Task<Configuration?> GetByApplicationAndNameAsync(string applicationId, string name);
    Task<IEnumerable<Configuration>> GetByApplicationIdAsync(string applicationId);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task<Configuration> UpdateAsync(Configuration configuration);
    Task DeleteAsync(string id);
}

