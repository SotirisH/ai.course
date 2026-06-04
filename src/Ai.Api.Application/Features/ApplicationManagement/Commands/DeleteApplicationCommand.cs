using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record DeleteApplicationCommand
{
    public Guid Id { get; init; }
}

public class DeleteApplicationCommandHandler
{
    public async Task Handle(
        DeleteApplicationCommand command,
        IApplicationRepository repository,
        CancellationToken cancellationToken)
    {
        DomainApp? application = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException($"Application with ID '{command.Id}' was not found.");
        }

        await repository.DeleteAsync(application, cancellationToken);
    }
}
