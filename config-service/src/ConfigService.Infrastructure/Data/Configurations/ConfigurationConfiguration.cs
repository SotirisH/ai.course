using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Data.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Configuration>
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.ToTable("configurations");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(c => c.ApplicationId)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(c => c.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(c => new { c.ApplicationId, c.Name })
            .IsUnique();
        
        builder.Property(c => c.Comments)
            .HasMaxLength(1024);
        
        builder.Property(c => c.Config)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}

