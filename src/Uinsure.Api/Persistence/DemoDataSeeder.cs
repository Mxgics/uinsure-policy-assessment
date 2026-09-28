using Microsoft.EntityFrameworkCore;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence;

public static class DemoDataSeeder
{
    public static readonly IReadOnlyList<string> References =
    [
        "POL-DEMO-AUTO-HH",
        "POL-DEMO-AUTO-BTL",
        "POL-DEMO-MANUAL-HH",
        "POL-DEMO-MANUAL-BTL",
        "POL-DEMO-CANCEL-REFUND",
        "POL-DEMO-CANCEL-CLAIMS"
    ];

    public static async Task SeedAsync(
        UinsureDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var renewalStart = today.AddYears(-1).AddDays(15);
        var cancellationStart = today.AddDays(-14);
        var references = References.ToArray();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var existing = await dbContext.Policies
            .Where(policy => references.Contains(policy.Reference))
            .ToArrayAsync(cancellationToken);
        dbContext.Policies.RemoveRange(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Policies.AddRange(
            Create("POL-DEMO-AUTO-HH", InsuranceType.Household, renewalStart, true, false,
                PaymentMethod.Card, 365m, "1 Automatic Avenue", "M1 1AA",
                Holder("Ada", "Automatic", 1987, 1, 10)),
            Create("POL-DEMO-AUTO-BTL", InsuranceType.BuyToLet, renewalStart, true, false,
                PaymentMethod.DirectDebit, 720m, "2 Renewal Road", "B1 2LT",
                Holder("Grace", "Example", 1985, 12, 9),
                Holder("Alan", "Example", 1982, 6, 23),
                Holder("Katherine", "Example", 1991, 8, 26)),
            Create("POL-DEMO-MANUAL-HH", InsuranceType.Household, renewalStart, false, false,
                PaymentMethod.Cheque, 410m, "3 Manual Mews", "LS1 3HH",
                Holder("Mary", "Manual", 1989, 4, 17),
                Holder("James", "Manual", 1986, 11, 5)),
            Create("POL-DEMO-MANUAL-BTL", InsuranceType.BuyToLet, renewalStart, false, false,
                PaymentMethod.Card, 680m, "4 Landlord Lane", "E1 4BT",
                Holder("Dorothy", "Manual", 1979, 5, 1)),
            Create("POL-DEMO-CANCEL-REFUND", InsuranceType.Household, cancellationStart, false, false,
                PaymentMethod.Card, 365m, "5 Refund Row", "M1 5CR",
                Holder("Margaret", "Refund", 1988, 7, 14),
                Holder("Edsger", "Refund", 1984, 9, 2)),
            Create("POL-DEMO-CANCEL-CLAIMS", InsuranceType.BuyToLet, cancellationStart, false, true,
                PaymentMethod.Cheque, 540m, "6 Claims Close", "L1 6CC",
                Holder("Barbara", "Claims", 1981, 3, 8),
                Holder("Donald", "Claims", 1983, 10, 12),
                Holder("Frances", "Claims", 1990, 2, 20)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        Policy Create(
            string reference,
            InsuranceType type,
            DateOnly startDate,
            bool autoRenew,
            bool hasClaims,
            PaymentMethod paymentMethod,
            decimal premium,
            string address,
            string postcode,
            params PolicyholderData[] holders) => Policy.Sell(
                reference,
                new SellPolicyData(
                    type,
                    startDate,
                    premium,
                    hasClaims,
                    autoRenew,
                    holders,
                    new PropertyData(address, null, null, "Exampleton", postcode),
                    paymentMethod),
                startDate,
                new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }

    private static PolicyholderData Holder(string firstName, string lastName, int year, int month, int day) =>
        new(firstName, lastName, new DateOnly(year, month, day));
}
