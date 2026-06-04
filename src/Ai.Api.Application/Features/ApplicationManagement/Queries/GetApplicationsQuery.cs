using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplicationsQuery;

public class GetApplicationsQueryHandler
{
    public async Task<IReadOnlyList<ApplicationDto>> Handle(
        GetApplicationsQuery query,
        IApplicationRepository repository,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DomainApp> applications = await repository.GetAllAsync(cancellationToken);
        return applications.ToDtoList();
    }
}
