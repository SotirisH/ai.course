using Ai.Api.Application.Interfaces.Repositories;
using Wolverine;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record DeleteApplication(Guid Id);

public class DeleteApplicationHandler
{
    private readonly IApplicationRepository _repository;

    public DeleteApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteApplication command, CancellationToken ct)
    {
        var application = await _repository.GetByIdAsync(command.Id, ct);
        if (application is null)
        {
            throw new ApplicationNotFoundException(command.Id);
        }

        await _repository.DeleteAsync(command.Id, ct);
    }
}
