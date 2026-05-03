namespace ConfigService.Application.DTOs;

public class ApplicationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public List<string> ConfigurationIds { get; set; } = new();
}

