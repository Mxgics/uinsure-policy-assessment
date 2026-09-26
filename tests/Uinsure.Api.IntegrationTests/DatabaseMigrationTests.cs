using Microsoft.EntityFrameworkCore;
using Uinsure.Api.IntegrationTests.Infrastructure;

namespace Uinsure.Api.IntegrationTests;

public sealed class DatabaseMigrationTests(SqlServerFixture fixture)
    : IClassFixture<SqlServerFixture>
{
    [Fact]
    public async Task Initial_migration_applies_to_real_sql_server()
    {
        await using var context = fixture.CreateContext();

        var applied = await context.Database.GetAppliedMigrationsAsync();

        Assert.Contains(applied, migration => migration.EndsWith("_InitialSchema", StringComparison.Ordinal));
        Assert.False((await context.Database.GetPendingMigrationsAsync()).Any());
    }
}
