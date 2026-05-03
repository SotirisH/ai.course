using Microsoft.EntityFrameworkCore;
using ConfigService.Domain.Entities;
using AppEntity = ConfigService.Domain.Entities.Application;

namespace ConfigService.Infrastructure.Data;

public class ConfigDbContext : DbContext
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options) : base(options)
    {
    }
    
    public DbSet<AppEntity> Applications { get; set; }
    public DbSet<Configuration> Configurations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigDbContext).Assembly);
    }
}
