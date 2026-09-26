using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.Reference).HasMaxLength(32).IsRequired();
        builder.HasIndex(policy => policy.Reference).IsUnique();
        builder.Property(policy => policy.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(policy => policy.RowVersion).IsRowVersion();
        builder.HasMany(policy => policy.Terms)
            .WithOne()
            .HasForeignKey(term => term.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
