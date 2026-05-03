namespace ConfigService.Application.DTOs;

public class UpdateConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}

