using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Infrastructure.Mappers;
using Ai.Api.Infrastructure.Persistence.Context;

namespace Ai.Api.Infrastructure.Persistence.Repositories;

public class ApplicationRepository(AppDbContext dbContext) : IApplicationRepository
{
    public async Task<ApplicationDto?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Entities.Application? entity = await dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Entities.Application> entities = await dbContext.Applications
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<ApplicationDto> AddAsync(CreateApplicationDto dto,
        CancellationToken cancellationToken = default)
    {
        Entities.Application entity = dto.ToEntity();

        await dbContext.Applications.AddAsync(entity, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"An application with the name '{dto.Name}' already exists.",
                ex);
        }

        return entity.ToDto();
    }

    public async Task<ApplicationDto> UpdateAsync(ApplicationDto dto,
        CancellationToken cancellationToken = default)
    {
        Entities.Application? entity = await dbContext.Applications
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"Application with ID '{dto.Id}' was not found.");
        }

        dto.ApplyTo(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"An application with the name '{dto.Name}' already exists.",
                ex);
        }

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Entities.Application? entity = await dbContext.Applications
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"Application with ID '{id}' was not found.");
        }

        dbContext.Applications.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsDuplicateKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
    }
}
