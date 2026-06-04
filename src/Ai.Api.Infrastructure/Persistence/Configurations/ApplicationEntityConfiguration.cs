using Ai.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Api.Infrastructure.Persistence.Configurations;

public class ApplicationEntityConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Comments)
            .HasMaxLength(1024);
    }
}
