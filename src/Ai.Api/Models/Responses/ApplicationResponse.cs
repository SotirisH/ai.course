namespace Ai.Api.Models.Responses;

public sealed record ApplicationResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
