namespace Ai.Api.Application.Interfaces.Repositories;

public interface IApplicationRepository
{
    Task<ApplicationDto?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ApplicationDto> AddAsync(CreateApplicationDto dto,
        CancellationToken cancellationToken = default);

    Task<ApplicationDto> UpdateAsync(ApplicationDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id,
        CancellationToken cancellationToken = default);
}
