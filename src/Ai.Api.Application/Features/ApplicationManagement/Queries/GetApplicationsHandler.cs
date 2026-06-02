using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Wolverine;

namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplications();

public class GetApplicationsHandler
{
    private readonly IApplicationRepository _repository;

    public GetApplicationsHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ApplicationDto>> Handle(GetApplications _, CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }
}
