namespace Ai.Api.Models.Requests;

public sealed record CreateApplicationRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
