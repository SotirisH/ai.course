using Ai.Api.Application.Features.ApplicationManagement.DTOs;

namespace Ai.Api.Infrastructure.Persistence;

internal static class ApplicationPersistenceMappingExtensions
{
    public static ApplicationDto ToDto(this Entities.Application entity)
    {
        return new ApplicationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Comments = entity.Comments
        };
    }

    public static Entities.Application ToEntity(this CreateApplicationDto dto)
    {
        return new Entities.Application
        {
            Id = Guid.CreateVersion7(),
            Name = dto.Name,
            Comments = dto.Comments
        };
    }

    public static void ApplyTo(this ApplicationDto dto, Entities.Application entity)
    {
        entity.Name = dto.Name;
        entity.Comments = dto.Comments;
    }
}
