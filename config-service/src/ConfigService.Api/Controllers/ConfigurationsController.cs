using Microsoft.AspNetCore.Mvc;
using ConfigService.Application.Interfaces;
using ConfigService.Application.DTOs;

namespace ConfigService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    
    public ConfigurationsController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> Create([FromBody] CreateConfigurationRequest request)
    {
        var result = await _configurationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ConfigurationDto>> Update(string id, [FromBody] UpdateConfigurationRequest request)
    {
        var result = await _configurationService.UpdateAsync(id, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDto>> GetById(string id)
    {
        var result = await _configurationService.GetByIdAsync(id);
        return Ok(result);
    }
}

