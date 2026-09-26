using Uinsure.Domain.Policies;

namespace Uinsure.Domain.Tests;

public sealed class PolicySaleTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Sale_creates_a_paid_one_year_term_and_normalizes_snapshots()
    {
        var policy = Policy.Sell(" pol-abc ", ValidData(), Today, Now);

        Assert.Equal("POL-ABC", policy.Reference);
        var term = Assert.Single(policy.Terms);
        Assert.Equal(new DateOnly(2027, 9, 30), term.EndDate);
        Assert.Equal("M1 1AA", term.Property.Postcode);
        Assert.Equal("Zach", Assert.Single(term.Policyholders).FirstName);
        Assert.Equal(365.00m, term.Payment!.Amount);
        Assert.Equal(Now, term.Payment.RecordedAtUtc);
    }

    [Fact]
    public void Leap_day_term_uses_calendar_year_boundary()
    {
        var policy = Policy.Sell(
            "POL-LEAP",
            ValidData(startDate: new DateOnly(2028, 2, 29)),
            new DateOnly(2028, 1, 1),
            Now);

        Assert.Equal(new DateOnly(2029, 2, 27), Assert.Single(policy.Terms).EndDate);
    }

    [Theory]
    [InlineData("2026-09-30", false)]
    [InlineData("2026-10-01", true)]
    [InlineData("2026-11-30", true)]
    [InlineData("2026-12-01", false)]
    public void Start_date_has_inclusive_today_and_plus_sixty_boundaries(string value, bool accepted)
    {
        var action = () => Policy.Sell(
            "POL-DATE",
            ValidData(startDate: DateOnly.Parse(value)),
            Today,
            Now);

        if (accepted) action();
        else Assert.Contains("startDate", Assert.Throws<DomainValidationException>(action).Errors.Keys);
    }

    [Theory]
    [InlineData("2010-10-01", true)]
    [InlineData("2010-10-02", false)]
    public void Exact_sixteenth_birthday_is_eligible(string dateOfBirth, bool accepted)
    {
        var data = ValidData(holderBirthDate: DateOnly.Parse(dateOfBirth));
        var action = () => Policy.Sell("POL-AGE", data, Today, Now);

        if (accepted) action();
        else Assert.Throws<DomainValidationException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Holder_count_outside_one_to_three_is_rejected(int count)
    {
        var data = ValidData() with
        {
            Policyholders = Enumerable.Range(0, count)
                .Select(_ => new PolicyholderData("A", "B", new DateOnly(1990, 1, 1)))
                .ToArray()
        };

        Assert.Contains(
            "policyholders",
            Assert.Throws<DomainValidationException>(() => Policy.Sell("POL-HOLDERS", data, Today, Now)).Errors.Keys);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.001")]
    public void Invalid_premium_is_rejected(string value)
    {
        var data = ValidData() with { Premium = decimal.Parse(value) };
        Assert.Throws<DomainValidationException>(() => Policy.Sell("POL-MONEY", data, Today, Now));
    }

    private static SellPolicyData ValidData(
        DateOnly? startDate = null,
        DateOnly? holderBirthDate = null) => new(
        InsuranceType.Household,
        startDate ?? Today,
        365m,
        HasClaims: false,
        AutoRenew: true,
        [new PolicyholderData(" Zach ", " Johnson ", holderBirthDate ?? new DateOnly(1990, 1, 1))],
        new PropertyData(" 1 Test Street ", null, " Manchester ", " m1 1aa ", 3),
        PaymentMethod.Card);
}
