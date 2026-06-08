namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record DeleteApplicationCommand
{
    public Guid Id { get; init; }
}

public class DeleteApplicationCommandHandler(IApplicationRepository repository)
{
    public async Task Handle(
        DeleteApplicationCommand command,
        CancellationToken cancellationToken)
    {
        ApplicationDto? existing = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException($"Application with ID '{command.Id}' was not found.");
        }

        await repository.DeleteAsync(command.Id, cancellationToken);
    }
}
