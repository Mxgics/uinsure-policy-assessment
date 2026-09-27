using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class ApiContractTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_is_process_liveness_only()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Healthy", document.GetProperty("status").GetString());
    }

    [Fact]
    public async Task OpenApi_describes_the_health_contract()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>(
            "/openapi/v1.json");

        Assert.True(document.GetProperty("paths").TryGetProperty("/health", out _));
    }

    [Fact]
    public async Task OpenApi_preserves_named_enums_and_the_property_contract()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var schemas = document.GetProperty("components").GetProperty("schemas");
        var payment = schemas.GetProperty("PaymentMethod");
        Assert.Equal("string", payment.GetProperty("type").GetString());
        Assert.Equal(new[] { "Card", "DirectDebit", "Cheque" }, payment.GetProperty("enum")
            .EnumerateArray().Select(v => v.GetString()).ToArray());
        Assert.Equal("string", schemas.GetProperty("InsuranceType").GetProperty("type").GetString());
        var property = schemas.GetProperty("PropertyRequest");
        Assert.True(property.GetProperty("properties").TryGetProperty("addressLine3", out _));
        Assert.False(property.GetProperty("properties").TryGetProperty("bedrooms", out _));
        var required = property.GetProperty("required").EnumerateArray().Select(v => v.GetString()).ToArray();
        Assert.Contains("addressLine1", required);
        Assert.Contains("postcode", required);
        Assert.DoesNotContain("city", required);
    }

    [Fact]
    public async Task Automatic_validation_uses_the_shared_problem_contract()
    {
        var response = await _client.PostAsJsonAsync(
            "/__tests/validation",
            new { name = (string?)null, count = 4 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation_error", problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        Assert.True(problem.GetProperty("errors").TryGetProperty("Name", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("Count", out _));
    }

    [Fact]
    public async Task Unknown_route_uses_the_shared_problem_contract()
    {
        var response = await _client.GetAsync("/not-a-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("not_found", problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Unexpected_exception_hides_internal_details()
    {
        var response = await _client.GetAsync("/__tests/exception");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(body);
        Assert.Equal("internal_error", problem.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain("Sensitive test exception text", body, StringComparison.Ordinal);
    }
}
