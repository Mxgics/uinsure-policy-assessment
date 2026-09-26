using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PropertySnapshotConfiguration : IEntityTypeConfiguration<PropertySnapshot>
{
    public void Configure(EntityTypeBuilder<PropertySnapshot> builder)
    {
        builder.ToTable("Properties", table =>
            table.HasCheckConstraint("CK_Properties_PositiveBedrooms", "[Bedrooms] > 0"));
        builder.HasKey(property => property.Id);
        builder.HasIndex(property => property.PolicyTermId).IsUnique();
        builder.Property(property => property.AddressLine1).HasMaxLength(200).IsRequired();
        builder.Property(property => property.AddressLine2).HasMaxLength(200);
        builder.Property(property => property.City).HasMaxLength(100).IsRequired();
        builder.Property(property => property.Postcode).HasMaxLength(8).IsRequired();
    }
}
