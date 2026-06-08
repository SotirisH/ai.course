namespace Ai.Api.Models.Responses;

public sealed record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
