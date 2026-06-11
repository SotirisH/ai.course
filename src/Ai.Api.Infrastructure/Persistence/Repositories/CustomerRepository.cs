using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Infrastructure.Persistence.Context;

namespace Ai.Api.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task<CustomerDto?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Entities.Customers? entity = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Entities.Customers> entities = await dbContext.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<CustomerDto> AddAsync(CreateCustomerDto dto,
        CancellationToken cancellationToken = default)
    {
        Entities.Customers entity = dto.ToEntity();

        await dbContext.Customers.AddAsync(entity, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"A customer with the tax ID '{dto.TaxId}' already exists.",
                ex);
        }

        return entity.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(CustomerDto dto,
        CancellationToken cancellationToken = default)
    {
        Entities.Customers? entity = await dbContext.Customers
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"Customer with ID '{dto.Id}' was not found.");
        }

        dto.ApplyTo(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            throw new InvalidOperationException(
                $"A customer with the tax ID '{dto.TaxId}' already exists.",
                ex);
        }

        return entity.ToDto();
    }

    private static bool IsDuplicateKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
    }
}
