namespace Ai.Api.Application.Features.ApplicationManagement.DTOs;

public sealed record CreateApplicationDto
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
