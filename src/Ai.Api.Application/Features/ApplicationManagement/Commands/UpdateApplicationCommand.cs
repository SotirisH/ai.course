using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record UpdateApplicationCommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class UpdateApplicationCommandHandler
{
    public async Task<DomainApp> Handle(
        UpdateApplicationCommand command,
        IApplicationRepository repository,
        CancellationToken cancellationToken)
    {
        DomainApp? application = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException($"Application with ID '{command.Id}' was not found.");
        }

        application.Update(command.Name, command.Comments);
        await repository.UpdateAsync(application, cancellationToken);
        return application;
    }
}
