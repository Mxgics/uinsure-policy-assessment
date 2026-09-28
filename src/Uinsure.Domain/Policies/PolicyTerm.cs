namespace Uinsure.Domain.Policies;

public sealed class PolicyTerm
{
    private readonly List<PolicyholderSnapshot> _policyholders = [];

    private PolicyTerm() { }

    public Guid Id { get; private set; }
    public Guid PolicyId { get; private set; }
    public Guid? PredecessorTermId { get; private set; }
    public Guid? PredecessorPolicyId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal Premium { get; private set; }
    public bool HasClaims { get; private set; }
    public bool AutoRenew { get; private set; }
    public IReadOnlyCollection<PolicyholderSnapshot> Policyholders => _policyholders;
    public PropertySnapshot Property { get; private set; } = null!;
    public Payment? Payment { get; private set; }
    public Cancellation? Cancellation { get; private set; }

    internal static PolicyTerm CreateInitial(
        Guid policyId,
        SellPolicyData data,
        DateTimeOffset recordedAtUtc)
    {
        var term = new PolicyTerm
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            StartDate = data.StartDate,
            EndDate = data.StartDate.AddYears(1).AddDays(-1),
            Premium = data.Premium,
            HasClaims = data.HasClaims,
            AutoRenew = data.AutoRenew
        };
        term._policyholders.AddRange(data.Policyholders.Select(holder =>
            PolicyholderSnapshot.Create(term.Id, holder)));
        term.Property = PropertySnapshot.Create(term.Id, data.Property);
        term.Payment = Payment.Create(term.Id, data.Premium, data.PaymentMethod, recordedAtUtc);
        return term;
    }

    internal static PolicyTerm CreateRenewal(
        Guid policyId,
        PolicyTerm predecessor,
        PaymentMethod? paymentMethod,
        DateTimeOffset recordedAtUtc)
    {
        var startDate = predecessor.EndDate.AddDays(1);
        var term = new PolicyTerm
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            PredecessorTermId = predecessor.Id,
            PredecessorPolicyId = policyId,
            StartDate = startDate,
            EndDate = startDate.AddYears(1).AddDays(-1),
            Premium = predecessor.Premium,
            HasClaims = false,
            AutoRenew = predecessor.AutoRenew
        };
        term._policyholders.AddRange(predecessor.Policyholders.Select(holder =>
            PolicyholderSnapshot.Create(term.Id, new PolicyholderData(
                holder.FirstName,
                holder.LastName,
                holder.DateOfBirth))));
        term.Property = PropertySnapshot.Create(term.Id, new PropertyData(
            predecessor.Property.AddressLine1,
            predecessor.Property.AddressLine2,
            predecessor.Property.AddressLine3,
            predecessor.Property.City,
            predecessor.Property.Postcode));
        if (paymentMethod is not null)
        {
            term.Payment = Payment.Create(term.Id, term.Premium, paymentMethod.Value, recordedAtUtc);
        }
        return term;
    }

    internal Cancellation Cancel(DateOnly date, DateTimeOffset recordedAtUtc)
    {
        if (Cancellation is not null)
        {
            throw new DomainConflictException("The policy term has already been cancelled.");
        }
        if (date > EndDate)
        {
            throw new DomainConflictException("An expired policy term cannot be cancelled.");
        }

        var calculation = CancellationCalculator.Calculate(
            StartDate,
            EndDate,
            Premium,
            HasClaims,
            Payment is null ? null : new RecordedPayment(Payment.Amount, Payment.Method),
            date);
        Cancellation = Uinsure.Domain.Policies.Cancellation.Create(Id, calculation, Payment, recordedAtUtc);
        return Cancellation;
    }

    public CancellationCalculation QuoteCancellation(DateOnly date)
    {
        if (Cancellation is not null)
        {
            throw new DomainConflictException("The policy term has already been cancelled.");
        }
        return CancellationCalculator.Calculate(
            StartDate,
            EndDate,
            Premium,
            HasClaims,
            Payment is null ? null : new RecordedPayment(Payment.Amount, Payment.Method),
            date);
    }

    public PolicyState StateOn(DateOnly date) => Cancellation is not null
        ? PolicyState.Cancelled
        : date < StartDate ? PolicyState.Scheduled : date > EndDate ? PolicyState.Expired : PolicyState.Current;
}
