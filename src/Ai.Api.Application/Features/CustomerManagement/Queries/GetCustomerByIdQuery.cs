using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.CustomerManagement.Queries;

public sealed record GetCustomerByIdQuery
{
    public Guid Id { get; init; }
}

public class GetCustomerByIdQueryHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken)
    {
        CustomerDto? dto = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (dto is null)
        {
            throw new InvalidOperationException($"Customer with ID '{query.Id}' was not found.");
        }

        return dto;
    }
}
