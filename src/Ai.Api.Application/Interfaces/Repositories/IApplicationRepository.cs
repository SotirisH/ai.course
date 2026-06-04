namespace Ai.Api.Application.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<DomainApp?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DomainApp>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(DomainApp application,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(DomainApp application,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(DomainApp application,
        CancellationToken cancellationToken = default);
}
