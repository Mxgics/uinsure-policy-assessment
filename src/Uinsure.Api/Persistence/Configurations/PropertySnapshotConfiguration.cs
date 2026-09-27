using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PropertySnapshotConfiguration : IEntityTypeConfiguration<PropertySnapshot>
{
    public void Configure(EntityTypeBuilder<PropertySnapshot> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(property => property.Id);
        builder.HasIndex(property => property.PolicyTermId).IsUnique();
        builder.Property(property => property.AddressLine1).HasMaxLength(PolicyLimits.AddressLine).IsRequired();
        builder.Property(property => property.AddressLine2).HasMaxLength(PolicyLimits.AddressLine);
        builder.Property(property => property.AddressLine3).HasMaxLength(PolicyLimits.AddressLine);
        builder.Property(property => property.City).HasMaxLength(PolicyLimits.City);
        builder.Property(property => property.Postcode).HasMaxLength(PolicyLimits.Postcode).IsRequired();
    }
}
