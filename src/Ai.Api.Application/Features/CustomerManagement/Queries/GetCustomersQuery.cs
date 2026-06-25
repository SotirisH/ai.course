using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;

namespace Ai.Api.Application.Features.CustomerManagement.Queries;

public sealed record GetCustomersQuery;

public class GetCustomersQueryHandler(ICustomerRepository repository)
{
    public async Task<IReadOnlyList<CustomerDto>> Handle(
        GetCustomersQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}
