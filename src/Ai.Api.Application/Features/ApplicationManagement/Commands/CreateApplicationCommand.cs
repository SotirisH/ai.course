using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record CreateApplicationCommand
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class CreateApplicationCommandHandler(IApplicationRepository repository)
{
    public async Task<ApplicationDto> Handle(
        CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        CreateApplicationDto dto = command.ToDto();

        return await repository.AddAsync(dto, cancellationToken);
    }
}
