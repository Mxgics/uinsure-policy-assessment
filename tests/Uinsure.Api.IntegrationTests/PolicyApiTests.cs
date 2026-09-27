using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class PolicyApiTests : IClassFixture<SqlServerFixture>, IAsyncLifetime
{
    private readonly SqlServerFixture _sql;
    private readonly SqlApiFactory _factory;
    private readonly HttpClient _client;

    public PolicyApiTests(SqlServerFixture sql)
    {
        _sql = sql;
        _factory = new SqlApiFactory(
            sql.ConnectionString,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)));
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Sell_then_retrieve_returns_persisted_snapshots_and_location()
    {
        var response = await _client.PostAsJsonAsync("/api/policies", ValidSale());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var reference = created.GetProperty("reference").GetString()!;
        var term = created.GetProperty("terms")[0];
        Assert.Equal($"/api/policies/{reference}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("2027-09-30", term.GetProperty("endDate").GetString());
        Assert.Equal("Current", term.GetProperty("state").GetString());
        Assert.Equal("Recorded", term.GetProperty("paymentState").GetString());
        Assert.Equal("M1 1AA", term.GetProperty("property").GetProperty("postcode").GetString());

        var retrieved = await _client.GetFromJsonAsync<JsonElement>($"/api/policies/{reference.ToLowerInvariant()}");
        Assert.Equal(reference, retrieved.GetProperty("reference").GetString());

        var termId = term.GetProperty("id").GetGuid();
        var retrievedTerm = await _client.GetFromJsonAsync<JsonElement>($"/api/policies/{reference}/terms/{termId}");
        Assert.Equal(termId, retrievedTerm.GetProperty("id").GetGuid());

        await using var context = _sql.CreateContext();
        Assert.True(await context.Policies.AnyAsync(policy => policy.Reference == reference));
        Assert.True(await context.Payments.AnyAsync(payment => payment.PolicyTermId == termId));
    }

    [Fact]
    public async Task Missing_required_boolean_is_validation_problem_and_writes_nothing()
    {
        int policyCountBefore;
        await using (var beforeContext = _sql.CreateContext())
        {
            policyCountBefore = await beforeContext.Policies.CountAsync();
        }

        var sale = ValidSale();
        sale.Remove("hasClaims");

        var response = await _client.PostAsJsonAsync("/api/policies", sale);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_error", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("hasClaims", out _));

        await using var afterContext = _sql.CreateContext();
        Assert.Equal(policyCountBefore, await afterContext.Policies.CountAsync());
    }

    [Fact]
    public async Task Numeric_enum_is_rejected_by_json_contract()
    {
        const string json = """
            {"type":0,"startDate":"2026-10-01","premium":100,"hasClaims":false,"autoRenew":false,
             "policyholders":[{"firstName":"A","lastName":"B","dateOfBirth":"1990-01-01"}],
             "property":{"addressLine1":"1 Road","city":"Town","postcode":"M1 1AA","bedrooms":2},"paymentMethod":"Card"}
            """;

        var response = await _client.PostAsync(
            "/api/policies",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_error", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unknown_or_mismatched_resources_return_not_found()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/policies/POL-UNKNOWN")).StatusCode);

        var response = await _client.PostAsJsonAsync("/api/policies", ValidSale());
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var termId = created.GetProperty("terms")[0].GetProperty("id").GetGuid();
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _client.GetAsync($"/api/policies/POL-WRONG/terms/{termId}")).StatusCode);
    }

    private static Dictionary<string, object?> ValidSale() => new()
    {
        ["type"] = "Household",
        ["startDate"] = "2026-10-01",
        ["premium"] = 365.00m,
        ["hasClaims"] = false,
        ["autoRenew"] = true,
        ["policyholders"] = new[]
        {
            new { firstName = " Zach ", lastName = " Johnson ", dateOfBirth = "1990-01-01" }
        },
        ["property"] = new
        {
            addressLine1 = " 1 Test Street ",
            addressLine2 = (string?)null,
            city = " Manchester ",
            postcode = " m1 1aa ",
            bedrooms = 3
        },
        ["paymentMethod"] = "Card"
    };
}
