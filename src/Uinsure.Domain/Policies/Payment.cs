namespace Uinsure.Domain.Policies;

public sealed class Payment
{
    private Payment() { }

    public Guid Id { get; private set; }
    public Guid PolicyTermId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Payment Create(
        Guid termId,
        decimal amount,
        PaymentMethod method,
        DateTimeOffset recordedAtUtc) => new()
        {
            Id = Guid.NewGuid(),
            PolicyTermId = termId,
            Reference = $"PAY-{Guid.NewGuid():N}".ToUpperInvariant(),
            Method = method,
            Amount = amount,
            RecordedAtUtc = recordedAtUtc
        };
}
