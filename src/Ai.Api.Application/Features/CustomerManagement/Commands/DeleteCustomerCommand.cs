using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.CustomerManagement.Commands;

public sealed record DeleteCustomerCommand
{
    public Guid Id { get; init; }
}

public class DeleteCustomerCommandHandler(ICustomerRepository repository)
{
    public async Task Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken)
    {
        CustomerDto? existing = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException($"Customer with ID '{command.Id}' was not found.");
        }

        await repository.DeleteAsync(command.Id, cancellationToken);
    }
}
