using Uinsure.Domain.Policies;

namespace Uinsure.Domain.Tests;

public sealed class CancellationCalculatorTests
{
    private static readonly DateOnly Start = new(2026, 10, 1);
    private static readonly DateOnly End = new(2027, 9, 30);

    [Theory]
    [InlineData(2026, 9, 30, 365, CancellationReason.BeforeStart, 365)]
    [InlineData(2026, 10, 1, 365, CancellationReason.CoolingOff, 365)]
    [InlineData(2026, 10, 14, 365, CancellationReason.CoolingOff, 352)]
    [InlineData(2026, 10, 15, 351, CancellationReason.ProRata, 351)]
    [InlineData(2027, 9, 30, 1, CancellationReason.ProRata, 1)]
    public void Calculates_literal_refund_boundaries(
        int year,
        int month,
        int day,
        decimal expectedRefund,
        CancellationReason expectedReason,
        int expectedUnusedDays)
    {
        var result = CancellationCalculator.Calculate(
            Start,
            End,
            365m,
            hasClaims: false,
            new RecordedPayment(365m, PaymentMethod.Card),
            new DateOnly(year, month, day));

        Assert.Equal(expectedRefund, result.RefundAmount);
        Assert.Equal(expectedReason, result.Reason);
        Assert.Equal(expectedUnusedDays, result.UnusedDays);
        Assert.Equal(365m - expectedRefund, result.RetainedPremium);
    }

    [Theory]
    [InlineData(true, true, CancellationReason.HasClaims)]
    [InlineData(false, false, CancellationReason.NoPayment)]
    public void Claims_or_no_payment_take_precedence_and_return_zero(
        bool hasPayment,
        bool hasClaims,
        CancellationReason expectedReason)
    {
        var payment = hasPayment ? new RecordedPayment(365m, PaymentMethod.DirectDebit) : null;

        var result = CancellationCalculator.Calculate(Start, End, 365m, hasClaims, payment, Start);

        Assert.Equal(0m, result.RefundAmount);
        Assert.Equal(expectedReason, result.Reason);
        Assert.Equal(hasPayment ? 365m : 0m, result.RetainedPremium);
        Assert.Equal(hasPayment ? PaymentMethod.DirectDebit : null, result.Method);
    }

    [Fact]
    public void Uses_actual_leap_term_day_count()
    {
        var result = CancellationCalculator.Calculate(
            new DateOnly(2027, 10, 1),
            new DateOnly(2028, 9, 30),
            366m,
            false,
            new RecordedPayment(366m, PaymentMethod.Cheque),
            new DateOnly(2027, 10, 15));

        Assert.Equal(352m, result.RefundAmount);
        Assert.Equal(366, result.TotalDays);
        Assert.Equal(352, result.UnusedDays);
        Assert.Equal(PaymentMethod.Cheque, result.Method);
    }

    [Fact]
    public void Rounds_once_away_from_zero_and_can_round_to_zero()
    {
        var rounded = CancellationCalculator.RoundRefund(1.005m);
        var tiny = CancellationCalculator.Calculate(
            Start, End, 0.01m, false,
            new RecordedPayment(0.01m, PaymentMethod.Card), End);

        Assert.Equal(1.01m, rounded);
        Assert.Equal(0m, tiny.RefundAmount);
    }

    [Fact]
    public void Rejects_a_date_after_the_term()
    {
        Assert.Throws<DomainValidationException>(() => CancellationCalculator.Calculate(
            Start, End, 365m, false,
            new RecordedPayment(365m, PaymentMethod.Card), End.AddDays(1)));
    }
}
