namespace Ai.Api.Application.Features.CustomerManagement.DTOs;

public sealed record CustomerDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}