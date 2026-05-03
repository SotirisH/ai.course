using ConfigService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ConfigDbContextFactory : IDesignTimeDbContextFactory<ConfigDbContext>
{
    public ConfigDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConfigDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ai_course;Username=postgres;Password=postgres");

        return new ConfigDbContext(optionsBuilder.Options);
    }
}
