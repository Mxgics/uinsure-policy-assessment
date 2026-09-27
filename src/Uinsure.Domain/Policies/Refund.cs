namespace Uinsure.Domain.Policies;

public sealed class Refund
{
    private Refund() { }

    public Guid Id { get; private set; }
    public Guid CancellationId { get; private set; }
    public Guid PaymentId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Refund Create(
        Guid cancellationId,
        Payment payment,
        decimal amount,
        DateTimeOffset recordedAtUtc) => new()
        {
            Id = Guid.NewGuid(),
            CancellationId = cancellationId,
            PaymentId = payment.Id,
            Reference = $"REF-{Guid.NewGuid():N}".ToUpperInvariant(),
            Amount = amount,
            Method = payment.Method,
            RecordedAtUtc = recordedAtUtc
        };
}
