using Ai.Api.Application.Features.ApplicationManagement.Commands;
using Ai.Api.Application.Features.ApplicationManagement.DTOs;
using Ai.Api.Application.Features.ApplicationManagement.Queries;
using Ai.Api.Models.Requests;
using Ai.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ai.Api.Controllers;

[ApiController]
[Route("applications")]
public class ApplicationsController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateApplicationCommand
        {
            Name = request.Name,
            Comments = request.Comments
        };

        var application = await messageBus.InvokeAsync<Domain.Entities.Application>(
            command,
            cancellationToken);

        var response = new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name,
            Comments = application.Comments
        };

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = response.Id
            },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetApplicationsQuery();

        var results = await messageBus.InvokeAsync<IReadOnlyList<ApplicationDto>>(
            query,
            cancellationToken);

        List<ApplicationResponse> response = results.Select(dto => new ApplicationResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                Comments = dto.Comments
            })
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetApplicationByIdQuery
        {
            Id = id
        };

        var dto = await messageBus.InvokeAsync<ApplicationDto>(
            query,
            cancellationToken);

        var response = new ApplicationResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Comments = dto.Comments
        };

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateApplicationCommand
        {
            Id = id,
            Name = request.Name,
            Comments = request.Comments
        };

        var application = await messageBus.InvokeAsync<Domain.Entities.Application>(
            command,
            cancellationToken);

        var response = new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name,
            Comments = application.Comments
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteApplicationCommand
        {
            Id = id
        };

        await messageBus.InvokeAsync(command, cancellationToken);

        return NoContent();
    }
}
