namespace Ai.Api.Infrastructure.Persistence.Entities;

public class ApplicationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }
}
