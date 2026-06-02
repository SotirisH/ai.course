using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Api.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Entities.ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<Entities.ApplicationEntity> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Comments)
            .HasMaxLength(1024);

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
