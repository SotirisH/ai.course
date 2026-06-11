using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.CustomerManagement.Commands;

public sealed record UpdateCustomerCommand
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class UpdateCustomerCommandHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> Handle(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        CustomerDto? existing = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException($"Customer with ID '{command.Id}' was not found.");
        }

        CustomerDto updated = command.ApplyTo(existing);

        return await repository.UpdateAsync(updated, cancellationToken);
    }
}
