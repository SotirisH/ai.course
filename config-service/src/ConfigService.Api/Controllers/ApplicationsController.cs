using Microsoft.AspNetCore.Mvc;
using ConfigService.Application.Interfaces;
using ConfigService.Application.DTOs;

namespace ConfigService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    
    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create([FromBody] CreateApplicationRequest request)
    {
        var result = await _applicationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicationDto>> Update(string id, [FromBody] UpdateApplicationRequest request)
    {
        var result = await _applicationService.UpdateAsync(id, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDto>> GetById(string id)
    {
        var result = await _applicationService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetAll()
    {
        var result = await _applicationService.GetAllAsync();
        return Ok(result);
    }
}

