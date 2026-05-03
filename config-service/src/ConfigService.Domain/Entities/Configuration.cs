using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfigService.Domain.Entities;

public class Configuration
{
    [Key]
    [MaxLength(26)] // ULID is 26 characters
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    
    [Required]
    [MaxLength(26)]
    public string ApplicationId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Comments { get; set; }
    
    [Column(TypeName = "jsonb")]
    public Dictionary<string, string> Config { get; set; } = new();
    
    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;
}

