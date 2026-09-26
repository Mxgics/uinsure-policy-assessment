using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Uinsure.Api.IntegrationTests.Infrastructure;

public sealed class SqlApiFactory(
    string connectionString,
    TimeProvider timeProvider) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Uinsure"] = connectionString,
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(timeProvider);
        });
    }
}
