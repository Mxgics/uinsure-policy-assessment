using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PolicyTermConfiguration : IEntityTypeConfiguration<PolicyTerm>
{
    public void Configure(EntityTypeBuilder<PolicyTerm> builder)
    {
        builder.ToTable("PolicyTerms", table =>
        {
            table.HasCheckConstraint("CK_PolicyTerms_EndAfterStart", "[EndDate] >= [StartDate]");
            table.HasCheckConstraint("CK_PolicyTerms_PositivePremium", "[Premium] > 0");
        });
        builder.HasKey(term => term.Id);
        builder.Property(term => term.StartDate).HasColumnType("date");
        builder.Property(term => term.EndDate).HasColumnType("date");
        builder.Property(term => term.Premium).HasPrecision(18, 2);
        builder.HasMany(term => term.Policyholders)
            .WithOne()
            .HasForeignKey(holder => holder.PolicyTermId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(term => term.Property)
            .WithOne()
            .HasForeignKey<PropertySnapshot>(property => property.PolicyTermId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(term => term.Payment)
            .WithOne()
            .HasForeignKey<Payment>(payment => payment.PolicyTermId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
