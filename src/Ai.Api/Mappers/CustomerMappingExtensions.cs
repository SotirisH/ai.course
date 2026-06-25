using Ai.Api.Application.Features.CustomerManagement.Commands;
using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Models.Requests;

namespace Ai.Api.Mappers;

public static class CustomerMappingExtensions
{
    public static CreateCustomerCommand ToCommand(this CreateCustomerRequest request)
    {
        return new CreateCustomerCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            TaxId = request.TaxId,
            Comments = request.Comments
        };
    }

    public static UpdateCustomerCommand ToCommand(this UpdateCustomerRequest request, Guid id)
    {
        return new UpdateCustomerCommand
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TaxId = request.TaxId,
            Comments = request.Comments
        };
    }

    public static DeleteCustomerCommand ToCommand(this Guid id)
    {
        return new DeleteCustomerCommand
        {
            Id = id
        };
    }

    public static CustomerResponse ToResponse(this CustomerDto dto)
    {
        return new CustomerResponse
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TaxId = dto.TaxId,
            Comments = dto.Comments
        };
    }

    public static List<CustomerResponse> ToResponseList(this IEnumerable<CustomerDto> dtos)
    {
        return dtos.Select(d => d.ToResponse()).ToList();
    }
}