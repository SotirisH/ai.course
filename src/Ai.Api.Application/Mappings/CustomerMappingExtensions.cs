using Ai.Api.Application.Features.CustomerManagement.Commands;
using Ai.Api.Application.Features.CustomerManagement.DTOs;

namespace Ai.Api.Application.Mappings;

public static class CustomerMappingExtensions
{
    public static CreateCustomerDto ToDto(this CreateCustomerCommand command)
    {
        return new CreateCustomerDto
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            TaxId = command.TaxId,
            Comments = command.Comments
        };
    }

    public static CustomerDto ApplyTo(this UpdateCustomerCommand command, CustomerDto existing)
    {
        return existing with
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            TaxId = command.TaxId,
            Comments = command.Comments
        };
    }
}
