using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Application.Mappings;

namespace Ai.Api.Application.Features.CustomerManagement.Commands;

public sealed record CreateCustomerCommand
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public class CreateCustomerCommandHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        CreateCustomerDto dto = command.ToDto();

        return await repository.AddAsync(dto, cancellationToken);
    }
}
