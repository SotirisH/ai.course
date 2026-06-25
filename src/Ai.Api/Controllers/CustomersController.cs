using Ai.Api.Application.Features.CustomerManagement.Commands;
using Ai.Api.Application.Features.CustomerManagement.DTOs;
using Ai.Api.Application.Features.CustomerManagement.Queries;
using Ai.Api.Mappers;
using Ai.Api.Models.Requests;
using Wolverine;

namespace Ai.Api.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var dto = await messageBus.InvokeAsync<CustomerDto>(request.ToCommand(), cancellationToken);

        CustomerResponse response = dto.ToResponse();

        return CreatedAtAction(nameof(GetById),
            new
            {
                id = response.Id
            },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery();
        var results = await messageBus.InvokeAsync<IReadOnlyList<CustomerDto>>(query, cancellationToken);
        List<CustomerResponse> response = results.ToResponseList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public async Task<ActionResult<CustomerResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery
        {
            Id = id
        };

        var dto = await messageBus.InvokeAsync<CustomerDto>(query, cancellationToken);

        CustomerResponse response = dto.ToResponse();

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        UpdateCustomerCommand command = request.ToCommand(id);

        var dto = await messageBus.InvokeAsync<CustomerDto>(command, cancellationToken);

        CustomerResponse response = dto.ToResponse();

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await messageBus.InvokeAsync(new DeleteCustomerCommand { Id = id }, cancellationToken);

        return NoContent();
    }
}
