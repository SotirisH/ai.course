namespace Ai.Api.Application.Features.CustomerManagement.DTOs;

public sealed record CreateCustomerDto
{
    public string? FirstName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}