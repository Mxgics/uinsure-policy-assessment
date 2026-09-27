namespace Uinsure.Api.IntegrationTests.Infrastructure;

public sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => value;
}
