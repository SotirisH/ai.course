using System.ComponentModel.DataAnnotations;

namespace Ai.Api.Infrastructure.Persistence.Entities;

public class ApplicationEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Comments { get; set; }
}
