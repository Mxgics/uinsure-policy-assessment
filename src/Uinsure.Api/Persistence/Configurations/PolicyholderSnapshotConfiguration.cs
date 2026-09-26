using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PolicyholderSnapshotConfiguration : IEntityTypeConfiguration<PolicyholderSnapshot>
{
    public void Configure(EntityTypeBuilder<PolicyholderSnapshot> builder)
    {
        builder.ToTable("Policyholders");
        builder.HasKey(holder => holder.Id);
        builder.Property(holder => holder.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(holder => holder.LastName).HasMaxLength(100).IsRequired();
        builder.Property(holder => holder.DateOfBirth).HasColumnType("date");
    }
}
