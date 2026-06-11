using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Api.Infrastructure.Persistence.Configurations;

public class CustomerEntityConfiguration : IEntityTypeConfiguration<Entities.Customers>
{
    public void Configure(EntityTypeBuilder<Entities.Customers> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(256);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.TaxId)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(x => x.TaxId)
            .IsUnique();

        builder.Property(x => x.Comments)
            .HasMaxLength(1024);
    }
}
