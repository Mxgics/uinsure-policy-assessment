using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
            table.HasCheckConstraint("CK_Payments_PositiveAmount", "[Amount] > 0"));
        builder.HasKey(payment => payment.Id);
        builder.HasIndex(payment => payment.Reference).IsUnique();
        builder.HasIndex(payment => payment.PolicyTermId).IsUnique();
        builder.Property(payment => payment.Reference).HasMaxLength(40).IsRequired();
        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
    }
}
