using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds", table =>
            table.HasCheckConstraint("CK_Refunds_PositiveAmount", "[Amount] > 0"));
        builder.HasKey(refund => refund.Id);
        builder.Property(refund => refund.Reference).HasMaxLength(40).IsRequired();
        builder.HasIndex(refund => refund.Reference).IsUnique();
        builder.HasIndex(refund => refund.CancellationId).IsUnique();
        builder.Property(refund => refund.Amount).HasPrecision(18, 2);
        builder.Property(refund => refund.Method).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(refund => refund.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
