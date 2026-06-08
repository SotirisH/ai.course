using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record UpdateApplicationCommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class UpdateApplicationCommandHandler(IApplicationRepository repository)
{
    public async Task<ApplicationDto> Handle(
        UpdateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        ApplicationDto? existing = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException($"Application with ID '{command.Id}' was not found.");
        }

        ApplicationDto updated = command.ApplyTo(existing);

        return await repository.UpdateAsync(updated, cancellationToken);
    }
}
