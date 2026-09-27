using Uinsure.Domain.Policies;

namespace Uinsure.Domain.Tests;

public sealed class PolicyRenewalTests
{
    [Theory]
    [InlineData(2027, 8, 30, false)]
    [InlineData(2027, 8, 31, true)]
    [InlineData(2027, 9, 30, true)]
    [InlineData(2027, 10, 1, false)]
    public void Renewal_uses_inclusive_end_minus_30_window(
        int year,
        int month,
        int day,
        bool accepted)
    {
        var policy = SoldPolicy(autoRenew: true, hasClaims: false);
        var termId = policy.Terms.Single().Id;

        Action action = () => policy.Renew(
            termId,
            new DateOnly(year, month, day),
            PaymentMethod.Card,
            DateTimeOffset.UtcNow);

        if (accepted) action(); else Assert.Throws<DomainConflictException>(action);
    }

    [Fact]
    public void Renewal_copies_snapshots_and_resets_claims()
    {
        var policy = SoldPolicy(autoRenew: true, hasClaims: true);
        var original = policy.Terms.Single();

        var successor = policy.Renew(
            original.Id,
            new DateOnly(2027, 8, 31),
            PaymentMethod.DirectDebit,
            DateTimeOffset.UtcNow);

        Assert.Equal(original.Id, successor.PredecessorTermId);
        Assert.Equal(new DateOnly(2027, 10, 1), successor.StartDate);
        Assert.Equal(new DateOnly(2028, 9, 30), successor.EndDate);
        Assert.Equal(original.Premium, successor.Premium);
        Assert.False(successor.HasClaims);
        Assert.Equal(original.Policyholders.Single().FirstName, successor.Policyholders.Single().FirstName);
        Assert.Equal(original.Property.Postcode, successor.Property.Postcode);
        Assert.Equal(PaymentMethod.DirectDebit, successor.Payment?.Method);
        Assert.Equal(1, policy.MutationRevision);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, PaymentMethod.Cheque)]
    [InlineData(false, PaymentMethod.Card)]
    public void Contradictory_or_unsupported_payment_is_validation(
        bool autoRenew,
        PaymentMethod? method)
    {
        var policy = SoldPolicy(autoRenew, hasClaims: false);

        Assert.Throws<DomainValidationException>(() => policy.Renew(
            policy.Terms.Single().Id,
            new DateOnly(2027, 8, 31),
            method,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Manual_renewal_creates_an_unpaid_successor()
    {
        var policy = SoldPolicy(autoRenew: false, hasClaims: false);

        var successor = policy.Renew(
            policy.Terms.Single().Id,
            new DateOnly(2027, 8, 31),
            null,
            DateTimeOffset.UtcNow);

        Assert.Null(successor.Payment);
    }

    [Fact]
    public void A_term_that_ever_had_a_successor_cannot_be_renewed_again()
    {
        var policy = SoldPolicy(autoRenew: true, hasClaims: false);
        var original = policy.Terms.Single();
        policy.Renew(original.Id, new DateOnly(2027, 8, 31), PaymentMethod.Card, DateTimeOffset.UtcNow);

        Assert.Throws<DomainConflictException>(() => policy.Renew(
            original.Id,
            new DateOnly(2027, 9, 1),
            PaymentMethod.Card,
            DateTimeOffset.UtcNow));
    }

    private static Policy SoldPolicy(bool autoRenew, bool hasClaims) => Policy.Sell(
        "POL-RENEWAL",
        new SellPolicyData(
            InsuranceType.Household,
            new DateOnly(2026, 10, 1),
            365m,
            hasClaims,
            autoRenew,
            [new PolicyholderData("A", "B", new DateOnly(1990, 1, 1))],
            new PropertyData("1 Road", null, null, "Town", "M1 1AA"),
            PaymentMethod.Card),
        new DateOnly(2026, 10, 1),
        DateTimeOffset.UtcNow);
}
