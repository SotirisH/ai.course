using Ai.Api.Infrastructure.Persistence.Entities;

namespace Ai.Api.Infrastructure.Persistence;

internal static class ApplicationPersistenceMappingExtensions
{
    public static ApplicationEntity ToEntity(this DomainApp domain)
    {
        return new ApplicationEntity
        {
            Id = domain.Id,
            Name = domain.Name,
            Comments = domain.Comments
        };
    }

    public static DomainApp ToDomain(this ApplicationEntity entity)
    {
        return new DomainApp(entity.Id, entity.Name, entity.Comments);
    }
}
