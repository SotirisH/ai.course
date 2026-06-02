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
public class ApplicationsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public ApplicationsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateApplicationRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _bus.InvokeAsync<ApplicationDto>(
                new CreateApplication(request.Name, request.Comments), ct);
            var response = MapToResponse(dto);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ApplicationAlreadyExistsException ex)
        {
            return Conflict(new { title = ex.Message, name = ex.Name });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApplicationRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _bus.InvokeAsync<ApplicationDto>(
                new UpdateApplication(id, request.Name, request.Comments), ct);
            return Ok(MapToResponse(dto));
        }
        catch (ApplicationNotFoundException)
        {
            return NotFound();
        }
        catch (ApplicationAlreadyExistsException ex)
        {
            return Conflict(new { title = ex.Message, name = ex.Name });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _bus.InvokeAsync(new DeleteApplication(id), ct);
            return NoContent();
        }
        catch (ApplicationNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _bus.InvokeAsync<ApplicationDto?>(
            new GetApplicationById(id), ct);
        return dto is null ? NotFound() : Ok(MapToResponse(dto));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var dtos = await _bus.InvokeAsync<IReadOnlyList<ApplicationDto>>(
            new GetApplications(), ct);
        return Ok(dtos.Select(MapToResponse).ToList());
    }

    private static ApplicationResponse MapToResponse(ApplicationDto dto)
    {
        return new ApplicationResponse(dto.Id, dto.Name, dto.Comments);
    }
}
