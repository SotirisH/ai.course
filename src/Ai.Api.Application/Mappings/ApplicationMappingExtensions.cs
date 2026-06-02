using Ai.Api.Application.Features.ApplicationManagement.DTOs;

namespace Ai.Api.Application.Mappings;

public static class ApplicationMappingExtensions
{
    public static ApplicationDto ToDto(this Domain.Entities.Application domain)
    {
        return new ApplicationDto(domain.Id, domain.Name, domain.Comments);
    }
}
