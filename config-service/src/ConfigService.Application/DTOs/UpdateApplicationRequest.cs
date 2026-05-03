namespace ConfigService.Application.DTOs;

public class UpdateApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

