using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;
using Wolverine;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record CreateApplication(string Name, string? Comments);

public class CreateApplicationHandler
{
    private readonly IApplicationRepository _repository;

    public CreateApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto> Handle(CreateApplication command, CancellationToken ct)
    {
        if (await _repository.ExistsByNameAsync(command.Name, ct: ct))
        {
            throw new ApplicationAlreadyExistsException(command.Name);
        }

        var application = new Domain.Entities.Application(command.Name, command.Comments);
        return await _repository.AddAsync(application, ct);
    }
}

public class ApplicationAlreadyExistsException : Exception
{
    public string Name { get; }

    public ApplicationAlreadyExistsException(string name)
        : base($"An application with the name '{name}' already exists.")
    {
        Name = name;
    }
}
