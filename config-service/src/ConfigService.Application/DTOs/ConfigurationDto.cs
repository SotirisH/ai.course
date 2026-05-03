namespace ConfigService.Application.DTOs;

public class ConfigurationDto
{
    public string Id { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}

