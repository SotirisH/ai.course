using Ai.Api.Application.Features.ApplicationManagement.Commands;
using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Features.ApplicationManagement.Queries;
using Ai.Api.Mappers;
using Ai.Api.Models.Requests;
using Wolverine;

namespace Ai.Api.Controllers;

[ApiController]
[Route("applications")]
public class ApplicationsController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApplicationResponse>> Create([FromBody] CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var dto = await messageBus.InvokeAsync<ApplicationDto>(request.ToCommand(), cancellationToken);

        ApplicationResponse response = dto.ToResponse();

        return CreatedAtAction(nameof(GetById),
            new
            {
                id = response.Id
            },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetApplicationsQuery();
        var results = await messageBus.InvokeAsync<IReadOnlyList<ApplicationDto>>(query, cancellationToken);
        List<ApplicationResponse> response = results.ToResponseList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public async Task<ActionResult<ApplicationResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetApplicationByIdQuery
        {
            Id = id
        };

        var dto = await messageBus.InvokeAsync<ApplicationDto>(query, cancellationToken);

        ApplicationResponse response = dto.ToResponse();

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApplicationResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        UpdateApplicationCommand command = request.ToCommand(id);

        var dto = await messageBus.InvokeAsync<ApplicationDto>(command, cancellationToken);

        ApplicationResponse response = dto.ToResponse();

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        DeleteApplicationCommand command = id.ToCommand();

        await messageBus.InvokeAsync(command, cancellationToken);

        return NoContent();
    }
}
