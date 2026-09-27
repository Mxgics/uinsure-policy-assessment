using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class CancellationApiTests(SqlServerFixture sql) : IClassFixture<SqlServerFixture>
{
    [Fact]
    public async Task Quote_is_read_only_and_execution_recalculates_and_persists_history()
    {
        var sold = await SellAsync(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        long revisionBefore;
        await using (var before = sql.CreateContext())
        {
            revisionBefore = await before.Policies
                .Where(policy => policy.Reference == sold.Reference)
                .Select(policy => policy.MutationRevision)
                .SingleAsync();
        }

        await using (var quoteFactory = Factory(new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero)))
        using (var quoteClient = quoteFactory.CreateClient())
        {
            var quote = await quoteClient.GetFromJsonAsync<JsonElement>(
                $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellation-quote?date=2026-10-15");
            Assert.Equal(351m, quote.GetProperty("refundAmount").GetDecimal());
            Assert.Equal("ProRata", quote.GetProperty("reason").GetString());
        }

        await using (var afterQuote = sql.CreateContext())
        {
            Assert.Equal(0, await afterQuote.Cancellations.CountAsync());
            Assert.Equal(revisionBefore, await afterQuote.Policies
                .Where(policy => policy.Reference == sold.Reference)
                .Select(policy => policy.MutationRevision)
                .SingleAsync());
        }

        await using var cancelFactory = Factory(new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero));
        using var cancelClient = cancelFactory.CreateClient();
        var response = await cancelClient.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var applied = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(351m, applied.GetProperty("refundAmount").GetDecimal());
        Assert.Equal("Card", applied.GetProperty("method").GetString());
        Assert.Equal($"/api/policies/{sold.Reference}/terms/{sold.TermId}", response.Headers.Location?.AbsolutePath);

        var second = await cancelClient.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var retrieved = await cancelClient.GetFromJsonAsync<JsonElement>(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}");
        Assert.Equal("Cancelled", retrieved.GetProperty("state").GetString());
        Assert.Equal(351m, retrieved.GetProperty("cancellation").GetProperty("refund").GetProperty("amount").GetDecimal());

        await using var after = sql.CreateContext();
        Assert.Equal(1, await after.Cancellations.CountAsync(item => item.PolicyTermId == sold.TermId));
        Assert.Equal(1, await after.Refunds.CountAsync(refund => refund.Amount == 351m));
        Assert.Equal(revisionBefore + 1, await after.Policies
            .Where(policy => policy.Reference == sold.Reference)
            .Select(policy => policy.MutationRevision)
            .SingleAsync());
    }

    [Theory]
    [InlineData(true, false, "HasClaims")]
    [InlineData(false, true, "NoPayment")]
    public async Task Claims_or_no_payment_cancel_without_a_refund(
        bool hasClaims,
        bool removePayment,
        string expectedReason)
    {
        var sold = await SellAsync(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero), hasClaims);
        if (removePayment)
        {
            await using var setup = sql.CreateContext();
            await setup.Payments.Where(payment => payment.PolicyTermId == sold.TermId).ExecuteDeleteAsync();
        }

        await using var factory = Factory(new DateTimeOffset(2026, 10, 5, 9, 0, 0, TimeSpan.Zero));
        using var client = factory.CreateClient();
        var response = await client.PostAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var applied = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0m, applied.GetProperty("refundAmount").GetDecimal());
        Assert.Equal(expectedReason, applied.GetProperty("reason").GetString());
        Assert.Equal(JsonValueKind.Null, applied.GetProperty("refund").ValueKind);
    }

    [Fact]
    public async Task Missing_quote_date_is_validation_and_mismatched_term_is_not_found()
    {
        var sold = await SellAsync(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        await using var factory = Factory(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(
            $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellation-quote")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync(
            $"/api/policies/POL-WRONG/terms/{sold.TermId}/cancellations", null)).StatusCode);
    }

    [Fact]
    public async Task Concurrent_cancellations_have_one_winner_and_one_complete_financial_effect()
    {
        var sold = await SellAsync(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        var barrier = new LifecycleSaveBarrier();
        Action<IServiceCollection> addBarrier = services => services.AddSingleton<IInterceptor>(barrier);
        await using var firstFactory = new SqlApiFactory(
            sql.ConnectionString,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero)),
            addBarrier);
        await using var secondFactory = new SqlApiFactory(
            sql.ConnectionString,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero)),
            addBarrier);
        using var firstClient = firstFactory.CreateClient();
        using var secondClient = secondFactory.CreateClient();
        var path = $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations";

        var responses = await Task.WhenAll(
            firstClient.PostAsync(path, null),
            secondClient.PostAsync(path, null));

        Assert.Equal(
            [HttpStatusCode.Created, HttpStatusCode.Conflict],
            responses.Select(response => response.StatusCode).Order().ToArray());
        await using var final = sql.CreateContext();
        var cancellationId = await final.Cancellations
            .Where(item => item.PolicyTermId == sold.TermId)
            .Select(item => item.Id)
            .SingleAsync();
        Assert.Equal(1, await final.Refunds.CountAsync(refund => refund.CancellationId == cancellationId));
        Assert.Equal(1, await final.Policies
            .Where(policy => policy.Reference == sold.Reference)
            .Select(policy => policy.MutationRevision)
            .SingleAsync());
    }

    [Fact]
    public async Task Later_refund_write_failure_rolls_back_cancellation_and_policy_revision()
    {
        var sold = await SellAsync(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        int refundCountBefore;
        await using (var before = sql.CreateContext())
        {
            refundCountBefore = await before.Refunds.CountAsync();
        }
        await using (var setup = sql.CreateContext())
        {
            await setup.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER TR_Refunds_FailTest ON Refunds AFTER INSERT AS
                BEGIN
                    THROW 51000, 'Injected later write failure', 1;
                END
                """);
        }

        try
        {
            await using var factory = Factory(new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero));
            using var client = factory.CreateClient();
            var response = await client.PostAsync(
                $"/api/policies/{sold.Reference}/terms/{sold.TermId}/cancellations", null);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            await using var final = sql.CreateContext();
            Assert.Equal(0, await final.Cancellations.CountAsync(item => item.PolicyTermId == sold.TermId));
            Assert.Equal(refundCountBefore, await final.Refunds.CountAsync());
            Assert.Equal(0, await final.Policies
                .Where(policy => policy.Reference == sold.Reference)
                .Select(policy => policy.MutationRevision)
                .SingleAsync());
        }
        finally
        {
            await using var cleanup = sql.CreateContext();
            await cleanup.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS TR_Refunds_FailTest");
        }
    }

    private SqlApiFactory Factory(DateTimeOffset now) => new(sql.ConnectionString, new FixedTimeProvider(now));

    private async Task<(string Reference, Guid TermId)> SellAsync(DateTimeOffset now, bool hasClaims = false)
    {
        await using var factory = Factory(now);
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", new
        {
            type = "Household",
            startDate = "2026-10-01",
            premium = 365.00m,
            hasClaims,
            autoRenew = true,
            policyholders = new[] { new { firstName = "A", lastName = "B", dateOfBirth = "1990-01-01" } },
            property = new { addressLine1 = "1 Road", city = "Town", postcode = "M1 1AA", bedrooms = 2 },
            paymentMethod = "Card"
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (
            created.GetProperty("reference").GetString()!,
            created.GetProperty("terms")[0].GetProperty("id").GetGuid());
    }
}
