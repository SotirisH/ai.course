using Ai.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ai.Api.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
