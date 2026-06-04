using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplicationByIdQuery
{
    public Guid Id { get; init; }
}

public class GetApplicationByIdQueryHandler
{
    public async Task<ApplicationDto> Handle(
        GetApplicationByIdQuery query,
        IApplicationRepository repository,
        CancellationToken cancellationToken)
    {
        DomainApp? application = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException($"Application with ID '{query.Id}' was not found.");
        }

        return application.ToDto();
    }
}
