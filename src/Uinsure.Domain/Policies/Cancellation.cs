namespace Uinsure.Domain.Policies;

public sealed class Cancellation
{
    private Cancellation() { }

    public Guid Id { get; private set; }
    public Guid PolicyTermId { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public decimal RefundAmount { get; private set; }
    public decimal RetainedPremium { get; private set; }
    public CancellationReason Reason { get; private set; }
    public int TotalDays { get; private set; }
    public int UsedDays { get; private set; }
    public int UnusedDays { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public Refund? Refund { get; private set; }

    internal static Cancellation Create(
        Guid termId,
        CancellationCalculation calculation,
        Payment? payment,
        DateTimeOffset recordedAtUtc)
    {
        var cancellation = new Cancellation
        {
            Id = Guid.NewGuid(),
            PolicyTermId = termId,
            EffectiveDate = calculation.Date,
            RefundAmount = calculation.RefundAmount,
            RetainedPremium = calculation.RetainedPremium,
            Reason = calculation.Reason,
            TotalDays = calculation.TotalDays,
            UsedDays = calculation.UsedDays,
            UnusedDays = calculation.UnusedDays,
            RecordedAtUtc = recordedAtUtc
        };
        if (calculation.RefundAmount > 0m && payment is not null)
        {
            cancellation.Refund = Refund.Create(cancellation.Id, payment, calculation.RefundAmount, recordedAtUtc);
        }
        return cancellation;
    }
}
