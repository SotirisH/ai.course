using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;
using Wolverine;

namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplicationById(Guid Id);

public class GetApplicationByIdHandler
{
    private readonly IApplicationRepository _repository;

    public GetApplicationByIdHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto?> Handle(GetApplicationById query, CancellationToken ct)
    {
        var application = await _repository.GetByIdAsync(query.Id, ct);
        return application?.ToDto();
    }
}
