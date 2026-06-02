namespace Ai.Api.Application.Features.ApplicationManagement.DTOs;

public sealed record ApplicationDto(Guid Id, string Name, string? Comments);
