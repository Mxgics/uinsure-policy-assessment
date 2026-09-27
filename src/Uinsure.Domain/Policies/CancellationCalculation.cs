namespace Uinsure.Domain.Policies;

public sealed record RecordedPayment(decimal Amount, PaymentMethod Method);

public sealed record CancellationCalculation(
    DateOnly Date,
    decimal RefundAmount,
    decimal RetainedPremium,
    string Currency,
    PaymentMethod? Method,
    CancellationReason Reason,
    int TotalDays,
    int UsedDays,
    int UnusedDays);
