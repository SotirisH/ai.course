using System.ComponentModel.DataAnnotations;

namespace ConfigService.Domain.Entities;

public class Application
{
    [Key]
    [MaxLength(26)] // ULID is 26 characters
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Comments { get; set; }
    
    public ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();
}

