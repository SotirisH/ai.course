namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplicationsQuery;

public class GetApplicationsQueryHandler(IApplicationRepository repository)
{
    public async Task<IReadOnlyList<ApplicationDto>> Handle(
        GetApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}
