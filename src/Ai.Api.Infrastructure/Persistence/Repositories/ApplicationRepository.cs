using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Infrastructure.Persistence.Context;
using Ai.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ai.Api.Infrastructure.Persistence.Repositories;

public class ApplicationRepository(AppDbContext dbContext) : IApplicationRepository
{
    public async Task<DomainApp?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        ApplicationEntity? entity = await dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<DomainApp>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<ApplicationEntity> entities = await dbContext.Applications
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task AddAsync(DomainApp application,
        CancellationToken cancellationToken = default)
    {
        ApplicationEntity entity = application.ToEntity();

        await dbContext.Applications.AddAsync(entity, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"An application with the name '{application.Name}' already exists.",
                ex);
        }
    }

    public async Task UpdateAsync(DomainApp application,
        CancellationToken cancellationToken = default)
    {
        ApplicationEntity entity = application.ToEntity();

        dbContext.Applications.Update(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"An application with the name '{application.Name}' already exists.",
                ex);
        }
    }

    public async Task DeleteAsync(DomainApp application,
        CancellationToken cancellationToken = default)
    {
        ApplicationEntity entity = application.ToEntity();

        dbContext.Applications.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsDuplicateKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
    }
}
