using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;
using Wolverine;

namespace Ai.Api.Application.Features.ApplicationManagement.Commands;

public sealed record UpdateApplication(Guid Id, string Name, string? Comments);

public class UpdateApplicationHandler
{
    private readonly IApplicationRepository _repository;

    public UpdateApplicationHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationDto> Handle(UpdateApplication command, CancellationToken ct)
    {
        var application = await _repository.GetByIdAsync(command.Id, ct);
        if (application is null)
        {
            throw new ApplicationNotFoundException(command.Id);
        }

        if (await _repository.ExistsByNameAsync(command.Name, command.Id, ct))
        {
            throw new ApplicationAlreadyExistsException(command.Name);
        }

        application.Update(command.Name, command.Comments);
        return await _repository.UpdateAsync(application, ct);
    }
}

public class ApplicationNotFoundException : Exception
{
    public Guid Id { get; }

    public ApplicationNotFoundException(Guid id)
        : base($"Application with ID '{id}' was not found.")
    {
        Id = id;
    }
}
