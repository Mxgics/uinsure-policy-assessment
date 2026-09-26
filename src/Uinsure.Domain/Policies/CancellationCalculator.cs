namespace Uinsure.Domain.Policies;

public static class CancellationCalculator
{
    public static CancellationCalculation Calculate(
        DateOnly startDate,
        DateOnly endDate,
        decimal premium,
        bool hasClaims,
        RecordedPayment? payment,
        DateOnly cancellationDate)
    {
        if (cancellationDate > endDate)
        {
            throw new DomainValidationException(new Dictionary<string, string[]>
            {
                ["date"] = ["Cancellation date cannot be after the term end date."]
            });
        }

        var endExclusive = endDate.AddDays(1);
        var totalDays = endExclusive.DayNumber - startDate.DayNumber;
        var usedDays = Math.Clamp(cancellationDate.DayNumber - startDate.DayNumber, 0, totalDays);
        var unusedDays = totalDays - usedDays;

        if (payment is null)
        {
            return Result(0m, 0m, null, CancellationReason.NoPayment);
        }

        if (hasClaims)
        {
            return Result(0m, payment.Amount, payment.Method, CancellationReason.HasClaims);
        }

        if (cancellationDate < startDate)
        {
            return Result(payment.Amount, 0m, payment.Method, CancellationReason.BeforeStart);
        }

        if (cancellationDate <= startDate.AddDays(13))
        {
            return Result(payment.Amount, 0m, payment.Method, CancellationReason.CoolingOff);
        }

        var calculated = payment.Amount * unusedDays / totalDays;
        var refund = Math.Clamp(RoundRefund(calculated), 0m, payment.Amount);
        return Result(refund, payment.Amount - refund, payment.Method, CancellationReason.ProRata);

        CancellationCalculation Result(
            decimal refund,
            decimal retained,
            PaymentMethod? method,
            CancellationReason reason) => new(
                cancellationDate,
                refund,
                retained,
                "GBP",
                method,
                reason,
                totalDays,
                usedDays,
                unusedDays);
    }

    public static decimal RoundRefund(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
}
