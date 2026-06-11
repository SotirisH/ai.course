using Ai.Api.Application.Features.ApplicationManagement.DTOs;

namespace Ai.Api.Infrastructure.Mappers;

internal static class ApplicationPersistenceMappingExtensions
{
    public static ApplicationDto ToDto(this Persistence.Entities.Application entity)
    {
        return new ApplicationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Comments = entity.Comments
        };
    }

    public static Persistence.Entities.Application ToEntity(this CreateApplicationDto dto)
    {
        return new Persistence.Entities.Application
        {
            Id = Guid.CreateVersion7(),
            Name = dto.Name,
            Comments = dto.Comments
        };
    }

    public static void ApplyTo(this ApplicationDto dto, Persistence.Entities.Application entity)
    {
        entity.Name = dto.Name;
        entity.Comments = dto.Comments;
    }
}
