using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class ReviewRegressionTests(SqlServerFixture sql) : IClassFixture<SqlServerFixture>
{
    private SqlApiFactory Factory() => new(sql.ConnectionString,
        new FixedTimeProvider(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)));

    public static JsonObject Sale() => JsonNode.Parse("""
        {"type":"Household","startDate":"2026-10-01","premium":365,"hasClaims":false,"autoRenew":true,
        "policyholders":[{"firstName":"Ada","lastName":"Example","dateOfBirth":"1990-01-01"}],
        "property":{"addressLine1":"1 Test Road","city":"Town","postcode":"M1 1AA","bedrooms":2},
        "paymentMethod":"Card"}
        """)!.AsObject();

    [Theory]
    [InlineData("paymentMethod", "DirectDebit, Cheque")]
    [InlineData("paymentMethod", "Card, DirectDebit")]
    [InlineData("paymentMethod", "0")]
    [InlineData("paymentMethod", "Unknown")]
    [InlineData("paymentMethod", "")]
    [InlineData("type", "Household, BuyToLet")]
    [InlineData("type", "0")]
    public async Task Invalid_enum_names_are_validation_errors_without_writes(string field, string value)
    {
        var sale = Sale();
        sale[field] = value;
        await AssertRejected(sale);
    }

    [Theory]
    [InlineData("type")]
    [InlineData("paymentMethod")]
    public async Task Numeric_JSON_enums_are_rejected_without_writes(string field)
    {
        var sale = Sale();
        sale[field] = 1;
        await AssertRejected(sale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Null_holder_is_an_indexed_validation_error(int index)
    {
        var sale = Sale();
        var holder = sale["policyholders"]![0]!;
        sale["policyholders"] = new JsonArray(holder.DeepClone(), holder.DeepClone(), holder.DeepClone());
        sale["policyholders"]![index] = null;
        var problem = await AssertRejected(sale);
        Assert.NotNull(problem["errors"]![$"policyholders[{index}]"]);
    }

    [Theory]
    [InlineData("firstName", 100)]
    [InlineData("lastName", 100)]
    [InlineData("addressLine1", 200)]
    [InlineData("addressLine2", 200)]
    [InlineData("addressLine3", 200)]
    [InlineData("city", 100)]
    [InlineData("postcode", 8)]
    public async Task Storage_limits_are_checked_after_normalization(string field, int limit)
    {
        var sale = Sale();
        var target = field.EndsWith("Name", StringComparison.Ordinal)
            ? sale["policyholders"]![0]! : sale["property"]!;
        target[field] = new string('X', limit + 1);
        await AssertRejected(sale);
        target[field] = " " + new string('X', limit) + " ";
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("10000000000000000")]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("1.001")]
    public async Task Invalid_premium_is_rejected_before_SQL(string value)
    {
        var sale = Sale();
        sale["premium"] = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        await AssertRejected(sale);
    }

    [Fact]
    public async Task Maximum_premium_round_trips_exactly()
    {
        var sale = Sale();
        const decimal maximum = 9999999999999999.99m;
        sale["premium"] = maximum;
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var reference = created["reference"]!.GetValue<string>();
        var retrieved = (await client.GetFromJsonAsync<JsonObject>($"/api/policies/{reference}"))!;
        Assert.Equal(maximum, retrieved["terms"]![0]!["premium"]!.GetValue<decimal>());
        await using var db = sql.CreateContext();
        Assert.Equal(maximum, await db.Policies.Where(p => p.Reference == reference)
            .SelectMany(p => p.Terms).Select(t => t.Premium).SingleAsync());
    }

    [Theory]
    [InlineData("2027-01-01")]
    [InlineData("9999-12-31")]
    [InlineData("2026-02-30")]
    [InlineData("not-a-date")]
    public async Task Invalid_birth_dates_are_informative_without_writes(string date)
    {
        var sale = Sale();
        sale["policyholders"]![0]!["dateOfBirth"] = date;
        await AssertRejected(sale);
    }

    [Fact]
    public async Task Minimal_property_needs_only_line_one_and_postcode()
    {
        var sale = Sale();
        sale["property"] = new JsonObject { ["addressLine1"] = "1 Test Road", ["postcode"] = "M1 1AA" };
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var property = created["terms"]![0]!["property"]!.AsObject();
        Assert.True(property.ContainsKey("city"));
        Assert.True(property.ContainsKey("addressLine3"));
        Assert.Null(property["city"]);
        Assert.Null(property["addressLine3"]);
        Assert.False(property.ContainsKey("bedrooms"));
    }

    [Theory]
    [InlineData("Household", "Card")]
    [InlineData("BuyToLet", "DirectDebit")]
    [InlineData("Household", "Cheque")]
    public async Task Named_values_round_trip_canonically(string type, string method)
    {
        var sale = Sale();
        sale["type"] = " " + type.ToLowerInvariant() + " ";
        sale["paymentMethod"] = " " + method.ToLowerInvariant() + " ";
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var reference = created["reference"]!.GetValue<string>();
        var retrieved = (await client.GetFromJsonAsync<JsonObject>($"/api/policies/{reference}"))!;
        Assert.Equal(type, retrieved["type"]!.GetValue<string>());
        Assert.Equal(method, retrieved["terms"]![0]!["payment"]!["method"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("Card, DirectDebit")]
    [InlineData("DirectDebit, Cheque")]
    [InlineData("0")]
    [InlineData("Unknown")]
    public async Task Renewal_rejects_invalid_names_without_new_rows_or_revision(string method)
    {
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var created = (await (await client.PostAsJsonAsync("/api/policies", Sale())).Content.ReadFromJsonAsync<JsonObject>())!;
        var reference = created["reference"]!.GetValue<string>();
        var termId = created["terms"]![0]!["id"]!.GetValue<Guid>();
        await using var renewalFactory = new SqlApiFactory(sql.ConnectionString,
            new FixedTimeProvider(new DateTimeOffset(2027, 9, 1, 9, 0, 0, TimeSpan.Zero)));
        using var renewalClient = renewalFactory.CreateClient();
        var response = await renewalClient.PostAsJsonAsync($"/api/policies/{reference}/terms/{termId}/renewals",
            new { paymentMethod = method });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var db = sql.CreateContext();
        var policy = await db.Policies.Include(p => p.Terms).ThenInclude(t => t.Payment)
            .SingleAsync(p => p.Reference == reference);
        Assert.Equal(0, policy.MutationRevision);
        Assert.NotNull(Assert.Single(policy.Terms).Payment);
    }

    [Theory]
    [InlineData("policyholders")]
    [InlineData("property")]
    public async Task Null_required_collections_or_objects_are_validation(string field)
    {
        var sale = Sale();
        sale[field] = null;
        await AssertRejected(sale);
    }

    [Theory]
    [InlineData("firstName")]
    [InlineData("lastName")]
    [InlineData("dateOfBirth")]
    public async Task Missing_holder_fields_are_validation(string field)
    {
        var sale = Sale();
        sale["policyholders"]![0]!.AsObject().Remove(field);
        await AssertRejected(sale);
    }

    [Fact]
    public async Task Three_holders_and_all_address_lines_survive_renewal()
    {
        var sale = Sale();
        sale["property"]!["addressLine2"] = " Second line ";
        sale["property"]!["addressLine3"] = " Third line ";
        sale["property"]!["city"] = " ";
        var holder = sale["policyholders"]![0]!;
        sale["policyholders"] = new JsonArray(holder.DeepClone(), holder.DeepClone(), holder.DeepClone());
        sale["policyholders"]![1]!["firstName"] = "Grace";
        sale["policyholders"]![2]!["firstName"] = "Linus";
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var reference = created["reference"]!.GetValue<string>();
        var termId = created["terms"]![0]!["id"]!.GetValue<Guid>();
        await using var renewalFactory = new SqlApiFactory(sql.ConnectionString,
            new FixedTimeProvider(new DateTimeOffset(2027, 9, 1, 9, 0, 0, TimeSpan.Zero)));
        using var renewalClient = renewalFactory.CreateClient();
        var renewal = await renewalClient.PostAsJsonAsync($"/api/policies/{reference}/terms/{termId}/renewals",
            new { paymentMethod = "DirectDebit" });
        Assert.Equal(HttpStatusCode.Created, renewal.StatusCode);
        var retrieved = (await client.GetFromJsonAsync<JsonObject>($"/api/policies/{reference}"))!;
        Assert.Equal(2, retrieved["terms"]!.AsArray().Count);
        foreach (var term in retrieved["terms"]!.AsArray())
        {
            Assert.Equal("Second line", term!["property"]!["addressLine2"]!.GetValue<string>());
            Assert.Equal("Third line", term["property"]!["addressLine3"]!.GetValue<string>());
            Assert.Null(term["property"]!["city"]);
            Assert.Equal(new[] { "Ada", "Grace", "Linus" }, term["policyholders"]!.AsArray()
                .Select(h => h!["firstName"]!.GetValue<string>()).Order().ToArray());
        }
        await using var db = sql.CreateContext();
        var stored = await db.Policies.Include(p => p.Terms).ThenInclude(t => t.Property)
            .Include(p => p.Terms).ThenInclude(t => t.Policyholders).SingleAsync(p => p.Reference == reference);
        Assert.All(stored.Terms, t => Assert.Equal(3, t.Policyholders.Count));
    }

    private async Task<JsonObject> AssertRejected(JsonObject sale)
    {
        await using var before = sql.CreateContext();
        var counts = new[] { await before.Policies.CountAsync(), await before.PolicyTerms.CountAsync(),
            await before.Payments.CountAsync(), await before.Cancellations.CountAsync(), await before.Refunds.CountAsync() };
        await using var factory = Factory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/policies", sale);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.Equal("validation_error", problem["code"]!.GetValue<string>());
        Assert.NotEmpty(problem["errors"]!.AsObject());
        await using var after = sql.CreateContext();
        Assert.Equal(counts, new[] { await after.Policies.CountAsync(), await after.PolicyTerms.CountAsync(),
            await after.Payments.CountAsync(), await after.Cancellations.CountAsync(), await after.Refunds.CountAsync() });
        return problem;
    }
}
