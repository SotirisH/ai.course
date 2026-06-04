using Ai.Api.Application.Features.ApplicationManagement.DTOs;

namespace Ai.Api.Application.Mappings;

public static class ApplicationMappingExtensions
{
    public static ApplicationDto ToDto(this DomainApp entity)
    {
        return new ApplicationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Comments = entity.Comments
        };
    }

    public static IReadOnlyList<ApplicationDto> ToDtoList(this IEnumerable<DomainApp> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}
