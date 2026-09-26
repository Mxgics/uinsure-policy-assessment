using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
{
    public void Configure(EntityTypeBuilder<Cancellation> builder)
    {
        builder.ToTable("Cancellations", table =>
        {
            table.HasCheckConstraint("CK_Cancellations_NonNegativeRefund", "[RefundAmount] >= 0");
            table.HasCheckConstraint("CK_Cancellations_NonNegativeRetained", "[RetainedPremium] >= 0");
        });
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.PolicyTermId).IsUnique();
        builder.Property(item => item.EffectiveDate).HasColumnType("date");
        builder.Property(item => item.RefundAmount).HasPrecision(18, 2);
        builder.Property(item => item.RetainedPremium).HasPrecision(18, 2);
        builder.Property(item => item.Reason).HasConversion<string>().HasMaxLength(20);
        builder.HasOne(item => item.Refund)
            .WithOne()
            .HasForeignKey<Refund>(refund => refund.CancellationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
