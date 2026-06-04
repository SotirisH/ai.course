using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record CreateApplicationCommand
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class CreateApplicationCommandHandler
{
    public async Task<DomainApp> Handle(
        CreateApplicationCommand command,
        IApplicationRepository repository,
        CancellationToken cancellationToken)
    {
        var application = new DomainApp(Guid.CreateVersion7(), command.Name, command.Comments);
        await repository.AddAsync(application, cancellationToken);
        return application;
    }
}
