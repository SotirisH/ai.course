namespace ConfigService.Application.DTOs;

public class CreateApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

