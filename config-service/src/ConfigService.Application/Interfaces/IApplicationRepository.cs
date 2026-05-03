using ConfigService.Domain.Entities;

namespace ConfigService.Application.Interfaces;

public interface IApplicationRepository
{
    Task<Domain.Entities.Application?> GetByIdAsync(string id);
    Task<Domain.Entities.Application?> GetByNameAsync(string name);
    Task<IEnumerable<Domain.Entities.Application>> GetAllAsync();
    Task<Domain.Entities.Application> CreateAsync(Domain.Entities.Application application);
    Task<Domain.Entities.Application> UpdateAsync(Domain.Entities.Application application);
    Task DeleteAsync(string id);
}
