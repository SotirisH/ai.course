using Ai.Api.Application.Features.ApplicationManagement.DTOs;

namespace Ai.Api.Application.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default);
    Task<ApplicationDto> AddAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task<ApplicationDto> UpdateAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
