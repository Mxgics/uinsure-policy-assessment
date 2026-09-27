using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Data.SqlClient;
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

    [Fact]
    public async Task Property_upgrade_preserves_history_and_refuses_lossy_rollback()
    {
        await using var db = fixture.CreateContext();
        var migrator = db.GetService<IMigrator>();
        const string previous = "20260926165616_AddRenewals";
        await migrator.MigrateAsync(previous); // Empty rollback is permitted.
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @policy uniqueidentifier = NEWID(), @term uniqueidentifier = NEWID();
            INSERT Policies (Id, Reference, Type, MutationRevision) VALUES (@policy, 'POL-MIGRATION', 'Household', 0);
            INSERT PolicyTerms (Id, PolicyId, StartDate, EndDate, Premium, HasClaims, AutoRenew)
            VALUES (@term, @policy, '2026-10-01', '2027-09-30', 365, 0, 1);
            INSERT Properties (Id, PolicyTermId, AddressLine1, AddressLine2, City, Postcode, Bedrooms)
            VALUES (NEWID(), @term, '1 Historical Road', 'Second line', 'Town', 'M1 1AA', 3);
            INSERT Policyholders (Id, PolicyTermId, FirstName, LastName, DateOfBirth)
            VALUES (NEWID(), @term, 'Ada', 'Example', '1990-01-01');
            INSERT Payments (Id, PolicyTermId, Reference, Method, Amount, RecordedAtUtc)
            VALUES (NEWID(), @term, 'PAY-MIGRATION', 'Card', 365, '2026-10-01T09:00:00Z');
            """);
        await migrator.MigrateAsync();
        await using var fresh = fixture.CreateContext();
        var policy = await fresh.Policies.Include(p => p.Terms).ThenInclude(t => t.Property)
            .Include(p => p.Terms).ThenInclude(t => t.Payment)
            .Include(p => p.Terms).ThenInclude(t => t.Policyholders)
            .SingleAsync(p => p.Reference == "POL-MIGRATION");
        var term = Assert.Single(policy.Terms);
        Assert.Equal("1 Historical Road", term.Property.AddressLine1);
        Assert.Equal("Second line", term.Property.AddressLine2);
        Assert.Equal("Town", term.Property.City);
        Assert.Null(term.Property.AddressLine3);
        Assert.Equal(365m, term.Payment!.Amount);
        Assert.Equal("Ada", Assert.Single(term.Policyholders).FirstName);
        Assert.False(fresh.Database.HasPendingModelChanges());
        var exception = await Assert.ThrowsAsync<SqlException>(() => migrator.MigrateAsync(previous));
        Assert.Equal(51002, exception.Number);
        Assert.Empty(await fresh.Database.GetPendingMigrationsAsync());
        Assert.Equal(1, await fresh.Policies.CountAsync(p => p.Reference == "POL-MIGRATION"));
    }
}
