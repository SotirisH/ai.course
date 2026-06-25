using Ai.Api.Application.Features.CustomerManagement.DTOs;

namespace Ai.Api.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<CustomerDto?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CustomerDto> AddAsync(CreateCustomerDto dto,
        CancellationToken cancellationToken = default);

    Task<CustomerDto> UpdateAsync(CustomerDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id,
        CancellationToken cancellationToken = default);
}
