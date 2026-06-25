using System.ComponentModel.DataAnnotations;

namespace Ai.Api.Infrastructure.Persistence.Entities;

public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string? FirstName { get; set; }

    [MaxLength(256)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(16)]
    public string TaxId { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Comments { get; set; }
}
