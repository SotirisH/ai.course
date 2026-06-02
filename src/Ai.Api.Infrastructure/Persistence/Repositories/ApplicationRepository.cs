using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;
using Ai.Api.Infrastructure.Persistence.Context;
using Ai.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ai.Api.Infrastructure.Persistence.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _db;

    public ApplicationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Applications.FindAsync([id], ct);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Applications
            .Select(e => e.ToDto())
            .ToListAsync(ct);
    }

    public async Task<ApplicationDto> AddAsync(Domain.Entities.Application application, CancellationToken ct = default)
    {
        var entity = MapToEntity(application);
        _db.Applications.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task<ApplicationDto> UpdateAsync(Domain.Entities.Application application, CancellationToken ct = default)
    {
        var entity = await _db.Applications.FindAsync([application.Id], ct)
            ?? throw new InvalidOperationException($"Application with ID '{application.Id}' not found.");

        entity.Name = application.Name;
        entity.Comments = application.Comments;
        await _db.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Applications.FindAsync([id], ct);
        if (entity is not null)
        {
            _db.Applications.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Applications.Where(e => e.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    private static Domain.Entities.Application MapToDomain(ApplicationEntity entity)
    {
        return new Domain.Entities.Application(entity.Id, entity.Name, entity.Comments);
    }

    private static ApplicationEntity MapToEntity(Domain.Entities.Application domain)
    {
        return new ApplicationEntity
        {
            Id = domain.Id,
            Name = domain.Name,
            Comments = domain.Comments
        };
    }
}

public static class ApplicationEntityMappingExtensions
{
    public static ApplicationDto ToDto(this ApplicationEntity entity)
    {
        return new ApplicationDto(entity.Id, entity.Name, entity.Comments);
    }
}
