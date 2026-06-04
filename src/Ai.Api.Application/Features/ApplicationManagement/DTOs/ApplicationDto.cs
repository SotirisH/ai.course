namespace Ai.Api.Application.Features.ApplicationManagement.DTOs;

public sealed record ApplicationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
