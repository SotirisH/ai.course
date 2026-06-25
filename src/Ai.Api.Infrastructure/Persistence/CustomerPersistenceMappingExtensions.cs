using Ai.Api.Application.Features.CustomerManagement.DTOs;

namespace Ai.Api.Infrastructure.Persistence;

internal static class CustomerPersistenceMappingExtensions
{
    public static CustomerDto ToDto(this Entities.Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            TaxId = entity.TaxId,
            Comments = entity.Comments
        };
    }

    public static Entities.Customer ToEntity(this CreateCustomerDto dto)
    {
        return new Entities.Customer
        {
            Id = Guid.CreateVersion7(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TaxId = dto.TaxId,
            Comments = dto.Comments
        };
    }

    public static void ApplyTo(this CustomerDto dto, Entities.Customer entity)
    {
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.TaxId = dto.TaxId;
        entity.Comments = dto.Comments;
    }
}