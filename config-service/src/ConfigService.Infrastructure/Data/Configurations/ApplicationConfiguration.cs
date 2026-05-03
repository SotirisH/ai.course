using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigService.Domain.Entities;
using AppEntity = ConfigService.Domain.Entities.Application;

namespace ConfigService.Infrastructure.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<AppEntity>
{
    public void Configure(EntityTypeBuilder<AppEntity> builder)
    {
        builder.ToTable("applications");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(a => a.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(a => a.Name)
            .IsUnique();
        
        builder.Property(a => a.Comments)
            .HasMaxLength(1024);
        
        builder.HasMany(a => a.Configurations)
            .WithOne(c => c.Application)
            .HasForeignKey(c => c.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
