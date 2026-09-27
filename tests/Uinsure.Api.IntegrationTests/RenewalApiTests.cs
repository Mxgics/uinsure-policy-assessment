using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class RenewalApiTests(SqlServerFixture sql) : IClassFixture<SqlServerFixture>
{
    private static readonly DateTimeOffset SaleTime = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RenewalTime = new(2027, 8, 31, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(true, "Card", "Recorded")]
    [InlineData(true, "DirectDebit", "Recorded")]
    [InlineData(false, null, "NotRecorded")]
    public async Task Renewal_creates_the_expected_successor_and_history(
        bool autoRenew,
        string? paymentMethod,
        string expectedPaymentState)
    {
        var sold = await SellAsync(autoRenew);
        await using var factory = Factory(RenewalTime);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals",
            new { paymentMethod });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var successor = await response.Content.ReadFromJsonAsync<JsonElement>();
        var successorId = successor.GetProperty("id").GetGuid();
        Assert.Equal(sold.TermId, successor.GetProperty("predecessorTermId").GetGuid());
        Assert.Equal("2027-10-01", successor.GetProperty("startDate").GetString());
        Assert.Equal("2028-09-30", successor.GetProperty("endDate").GetString());
        Assert.False(successor.GetProperty("hasClaims").GetBoolean());
        Assert.Equal(expectedPaymentState, successor.GetProperty("paymentState").GetString());
        Assert.Equal($"/api/policies/{sold.Reference}/terms/{successorId}", response.Headers.Location?.AbsolutePath);

        var policy = await client.GetFromJsonAsync<JsonElement>($"/api/policies/{sold.Reference}");
        Assert.Equal(2, policy.GetProperty("terms").GetArrayLength());

        if (!autoRenew)
        {
            var cancellation = await client.PostAsync(
                $"/api/policies/{sold.Reference}/terms/{successorId}/cancellations", null);
            var applied = await cancellation.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("NoPayment", applied.GetProperty("reason").GetString());
            Assert.Equal(0m, applied.GetProperty("refundAmount").GetDecimal());
        }
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "Cheque")]
    [InlineData(false, "Card")]
    public async Task Invalid_payment_combination_is_validation_and_writes_no_successor(
        bool autoRenew,
        string? paymentMethod)
    {
        var sold = await SellAsync(autoRenew);
        await using var factory = Factory(RenewalTime);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals",
            new { paymentMethod });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var final = sql.CreateContext();
        Assert.Equal(1, await final.PolicyTerms.CountAsync(term => term.PolicyId == sold.PolicyId));
    }

    [Fact]
    public async Task Repeated_renewal_conflicts_and_a_cancelled_successor_is_not_replaced()
    {
        var sold = await SellAsync(autoRenew: true);
        await using var factory = Factory(RenewalTime);
        using var client = factory.CreateClient();
        var path = $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals";
        var first = await client.PostAsJsonAsync(path, new { paymentMethod = "Card" });
        var successor = await first.Content.ReadFromJsonAsync<JsonElement>();
        var successorId = successor.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(
            path, new { paymentMethod = "Card" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{successorId}/cancellations", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(
            path, new { paymentMethod = "Card" })).StatusCode);
    }

    [Fact]
    public async Task Concurrent_renewals_have_one_successor_and_one_payment()
    {
        var sold = await SellAsync(autoRenew: true);
        var barrier = new LifecycleSaveBarrier();
        Action<IServiceCollection> addBarrier = services => services.AddSingleton<IInterceptor>(barrier);
        await using var firstFactory = Factory(RenewalTime, addBarrier);
        await using var secondFactory = Factory(RenewalTime, addBarrier);
        using var firstClient = firstFactory.CreateClient();
        using var secondClient = secondFactory.CreateClient();
        var path = $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals";

        var responses = await Task.WhenAll(
            firstClient.PostAsJsonAsync(path, new { paymentMethod = "Card" }),
            secondClient.PostAsJsonAsync(path, new { paymentMethod = "Card" }));

        Assert.Equal(
            [HttpStatusCode.Created, HttpStatusCode.Conflict],
            responses.Select(response => response.StatusCode).Order().ToArray());
        await using var final = sql.CreateContext();
        var successors = await final.PolicyTerms
            .Where(term => term.PredecessorTermId == sold.TermId)
            .Select(term => term.Id)
            .ToArrayAsync();
        Assert.Single(successors);
        Assert.Equal(1, await final.Payments.CountAsync(payment => payment.PolicyTermId == successors[0]));
    }

    [Fact]
    public async Task Concurrent_cancel_and_renew_leave_only_one_lifecycle_effect()
    {
        var sold = await SellAsync(autoRenew: true);
        var barrier = new LifecycleSaveBarrier();
        Action<IServiceCollection> addBarrier = services => services.AddSingleton<IInterceptor>(barrier);
        await using var cancelFactory = Factory(RenewalTime, addBarrier);
        await using var renewFactory = Factory(RenewalTime, addBarrier);
        using var cancelClient = cancelFactory.CreateClient();
        using var renewClient = renewFactory.CreateClient();

        var responses = await Task.WhenAll(
            cancelClient.PostAsync(
                $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null),
            renewClient.PostAsJsonAsync(
                $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals",
                new { paymentMethod = "Card" }));

        Assert.Equal(
            [HttpStatusCode.Created, HttpStatusCode.Conflict],
            responses.Select(response => response.StatusCode).Order().ToArray());
        await using var final = sql.CreateContext();
        var cancelled = await final.Cancellations.AnyAsync(item => item.PolicyTermId == sold.TermId);
        var renewed = await final.PolicyTerms.AnyAsync(item => item.PredecessorTermId == sold.TermId);
        Assert.NotEqual(cancelled, renewed);
    }

    [Fact]
    public async Task Later_renewal_payment_failure_rolls_back_successor_and_revision()
    {
        var sold = await SellAsync(autoRenew: true);
        await using (var setup = sql.CreateContext())
        {
            await setup.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER TR_Payments_FailRenewalTest ON Payments AFTER INSERT AS
                BEGIN
                    THROW 51001, 'Injected renewal payment failure', 1;
                END
                """);
        }

        try
        {
            await using var factory = Factory(RenewalTime);
            using var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"/api/policies/{sold.Reference}/terms/{sold.TermId}/renewals",
                new { paymentMethod = "Card" });

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            await using var final = sql.CreateContext();
            Assert.False(await final.PolicyTerms.AnyAsync(term => term.PredecessorTermId == sold.TermId));
            Assert.Equal(0, await final.Policies
                .Where(policy => policy.Reference == sold.Reference)
                .Select(policy => policy.MutationRevision)
                .SingleAsync());
        }
        finally
        {
            await using var cleanup = sql.CreateContext();
            await cleanup.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS TR_Payments_FailRenewalTest");
        }
    }

    private SqlApiFactory Factory(DateTimeOffset now, Action<IServiceCollection>? configure = null) =>
        new(sql.ConnectionString, new FixedTimeProvider(now), configure);

    private async Task<(Guid PolicyId, string Reference, Guid TermId)> SellAsync(bool autoRenew)
    {
        await using var factory = Factory(SaleTime);
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", new
        {
            type = "BuyToLet",
            startDate = "2026-10-01",
            premium = 365.00m,
            hasClaims = true,
            autoRenew,
            policyholders = new[] { new { firstName = "A", lastName = "B", dateOfBirth = "1990-01-01" } },
            property = new { addressLine1 = "1 Road", city = "Town", postcode = "M1 1AA", bedrooms = 2 },
            paymentMethod = "Card"
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var reference = created.GetProperty("reference").GetString()!;
        await using var context = sql.CreateContext();
        var policyId = await context.Policies
            .Where(policy => policy.Reference == reference)
            .Select(policy => policy.Id)
            .SingleAsync();
        return (policyId, reference, created.GetProperty("terms")[0].GetProperty("id").GetGuid());
    }
}
