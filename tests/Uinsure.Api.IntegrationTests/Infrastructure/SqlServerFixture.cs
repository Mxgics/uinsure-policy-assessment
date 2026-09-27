using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Uinsure.Api.Persistence;

namespace Uinsure.Api.IntegrationTests.Infrastructure;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string Image =
        "mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04@sha256:4402d880dd4c34bfa7d8705e56a86cd6c88da80a1f6bbbe741f999e76264a090";

    private readonly MsSqlContainer _container = new MsSqlBuilder(Image)
        .WithPassword(CreatePassword())
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public UinsureDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UinsureDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new UinsureDbContext(options);
    }

    private static string CreatePassword() =>
        $"T!{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}a1";
}
