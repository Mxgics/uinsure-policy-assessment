using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Uinsure.Api.IntegrationTests.Infrastructure;
using Uinsure.Api.Persistence;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.IntegrationTests;

public sealed class DemoDataSeederTests(SqlServerFixture sql) : IClassFixture<SqlServerFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Seeding_is_repeatable_resets_only_demo_policies_and_creates_each_scenario()
    {
        var retainedReference = $"POL-KEEP-{Guid.NewGuid():N}"[..24].ToUpperInvariant();
        await using (var setup = sql.CreateContext())
        {
            setup.Policies.Add(Policy.Sell(
                retainedReference,
                Data(new DateOnly(2026, 10, 1), autoRenew: false, PaymentMethod.Cheque),
                new DateOnly(2026, 10, 1),
                Now));
            await setup.SaveChangesAsync();
            await DemoDataSeeder.SeedAsync(setup, new FixedTimeProvider(Now));
        }

        Guid automaticTermId;
        Guid cancellableTermId;
        await using (var ids = sql.CreateContext())
        {
            automaticTermId = await ids.PolicyTerms
                .Where(term => ids.Policies.Any(policy => policy.Reference == "POL-DEMO-AUTO-HH" && policy.Id == term.PolicyId))
                .Select(term => term.Id).SingleAsync();
            cancellableTermId = await ids.PolicyTerms
                .Where(term => ids.Policies.Any(policy => policy.Reference == "POL-DEMO-CANCEL-REFUND" && policy.Id == term.PolicyId))
                .Select(term => term.Id).SingleAsync();
        }
        await using (var factory = new SqlApiFactory(sql.ConnectionString, new FixedTimeProvider(Now)))
        using (var client = factory.CreateClient())
        {
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
                $"/api/policies/POL-DEMO-AUTO-HH/terms/{automaticTermId}/renewals",
                new { paymentMethod = "Card" })).StatusCode);
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsync(
                $"/api/policies/POL-DEMO-CANCEL-REFUND/terms/{cancellableTermId}/cancellations", null)).StatusCode);
        }
        await using (var reseed = sql.CreateContext())
        {
            await DemoDataSeeder.SeedAsync(reseed, new FixedTimeProvider(Now));
        }

        await using var verify = sql.CreateContext();
        var demos = await verify.Policies
            .AsNoTracking()
            .Include(policy => policy.Terms).ThenInclude(term => term.Policyholders)
            .Include(policy => policy.Terms).ThenInclude(term => term.Payment)
            .Include(policy => policy.Terms).ThenInclude(term => term.Cancellation)
            .Where(policy => DemoDataSeeder.References.Contains(policy.Reference))
            .OrderBy(policy => policy.Reference)
            .ToArrayAsync();

        Assert.Equal(6, demos.Length);
        Assert.All(demos, policy => Assert.Single(policy.Terms));
        Assert.All(demos, policy => Assert.Null(policy.Terms.Single().Cancellation));
        Assert.Equal([3, 1, 3, 2, 1, 2], demos.Select(policy => policy.Terms.Single().Policyholders.Count));
        Assert.True(await verify.Policies.AnyAsync(policy => policy.Reference == retainedReference));

        Assert.True(demos.Single(policy => policy.Reference == "POL-DEMO-AUTO-HH").Terms.Single().AutoRenew);
        Assert.True(demos.Single(policy => policy.Reference == "POL-DEMO-AUTO-BTL").Terms.Single().AutoRenew);
        Assert.Equal(PaymentMethod.Cheque,
            demos.Single(policy => policy.Reference == "POL-DEMO-MANUAL-HH").Terms.Single().Payment!.Method);
        Assert.True(demos.Single(policy => policy.Reference == "POL-DEMO-CANCEL-CLAIMS").Terms.Single().HasClaims);
    }

    [Fact]
    public async Task Seeding_rolls_back_the_demo_replacement_when_creation_fails()
    {
        await using (var setup = sql.CreateContext())
        {
            await DemoDataSeeder.SeedAsync(setup, new FixedTimeProvider(Now));
        }

        Dictionary<string, Guid> originalIds;
        await using (var before = sql.CreateContext())
        {
            originalIds = await before.Policies
                .Where(policy => DemoDataSeeder.References.Contains(policy.Reference))
                .ToDictionaryAsync(policy => policy.Reference, policy => policy.Id);
        }

        var options = new DbContextOptionsBuilder<UinsureDbContext>()
            .UseSqlServer(sql.ConnectionString)
            .AddInterceptors(new FailOnSecondSaveChangesInterceptor())
            .Options;
        await using (var failing = new UinsureDbContext(options))
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DemoDataSeeder.SeedAsync(failing, new FixedTimeProvider(Now)));
        }

        await using var verify = sql.CreateContext();
        var retainedIds = await verify.Policies
            .Where(policy => DemoDataSeeder.References.Contains(policy.Reference))
            .ToDictionaryAsync(policy => policy.Reference, policy => policy.Id);
        Assert.Equal(originalIds, retainedIds);
    }

    private static SellPolicyData Data(DateOnly start, bool autoRenew, PaymentMethod paymentMethod) => new(
        InsuranceType.Household,
        start,
        365m,
        false,
        autoRenew,
        [new PolicyholderData("Keep", "Example", new DateOnly(1990, 1, 1))],
        new PropertyData("1 Retained Road", null, null, null, "M1 1AA"),
        paymentMethod);

    private sealed class FailOnSecondSaveChangesInterceptor : SaveChangesInterceptor
    {
        private int _calls;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _calls) == 2)
            {
                throw new InvalidOperationException("Synthetic failure after deleting existing demo policies.");
            }

            return ValueTask.FromResult(result);
        }
    }

}
