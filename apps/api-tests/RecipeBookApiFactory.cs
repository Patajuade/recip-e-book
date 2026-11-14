using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Npgsql;

using RecipEBook.Api.Infrastructure.Persistence;

using Testcontainers.PostgreSql;

namespace RecipEBook.Api.IntegrationTests;

/// <summary>
/// Runs the API against an empty PostgreSQL database, created with the real migrations (so the
/// tag catalog is there). Replacing the DbContext registration also drops the development demo
/// seeding. One database per factory, so per test class (IClassFixture).
/// </summary>
public sealed class RecipeBookApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = TestDatabaseServer.NewDatabase();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs requires a connection string; on CI there are no user secrets to give one.
        builder.UseSetting("ConnectionStrings:RecipEBook", _connectionString);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        using IServiceScope scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

        return host;
    }
}

/// <summary>
/// One PostgreSQL container for the whole test run, started on first use. Testcontainers removes
/// it when the test process ends.
/// </summary>
internal static class TestDatabaseServer
{
    // The same major version as compose.yaml.
    private static readonly Lazy<PostgreSqlContainer> Container = new(() =>
    {
        PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18").Build();
        container.StartAsync().GetAwaiter().GetResult();
        return container;
    });

    public static string NewDatabase() =>
        new NpgsqlConnectionStringBuilder(Container.Value.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;
}
