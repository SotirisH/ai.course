using Ai.Api.Application.Features.ApplicationManagement.Commands;
using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Models.Requests;

namespace Ai.Api.Mappers;

public static class ApplicationMappingExtensions
{
    public static CreateApplicationCommand ToCommand(this CreateApplicationRequest request)
    {
        return new CreateApplicationCommand
        {
            Name = request.Name,
            Comments = request.Comments
        };
    }

    public static UpdateApplicationCommand ToCommand(this UpdateApplicationRequest request, Guid id)
    {
        return new UpdateApplicationCommand
        {
            Id = id,
            Name = request.Name,
            Comments = request.Comments
        };
    }

    public static DeleteApplicationCommand ToCommand(this Guid id)
    {
        return new DeleteApplicationCommand
        {
            Id = id
        };
    }

    public static ApplicationResponse ToResponse(this ApplicationDto dto)
    {
        return new ApplicationResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Comments = dto.Comments
        };
    }

    public static List<ApplicationResponse> ToResponseList(this IEnumerable<ApplicationDto> dtos)
    {
        return dtos.Select(d => d.ToResponse()).ToList();
    }
}
