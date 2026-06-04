namespace Ai.Api.Models.Requests;

public sealed record UpdateApplicationRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
