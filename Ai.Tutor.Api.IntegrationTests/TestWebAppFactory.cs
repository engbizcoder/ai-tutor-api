namespace Ai.Tutor.Api.IntegrationTests;

using Ai.Tutor.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer pg;

    public TestWebAppFactory()
    {
        var image = Environment.GetEnvironmentVariable("ITESTS_PG_IMAGE") ?? "postgres:17-alpine";
        var db = Environment.GetEnvironmentVariable("ITESTS_PG_DB") ?? "ai_tutor_test";
        var user = Environment.GetEnvironmentVariable("ITESTS_PG_USER") ?? "postgres";
        var pwd = Environment.GetEnvironmentVariable("ITESTS_PG_PASSWORD") ?? "postgres";

        this.pg = new PostgreSqlBuilder()
            .WithImage(image)
            .WithDatabase(db)
            .WithUsername(user)
            .WithPassword(pwd)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start container
        await this.pg.StartAsync().ConfigureAwait(false);

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Default",
            this.pg.GetConnectionString());

        // Build the server/host and apply migrations
        _ = Server; // ensure host is created
        using var scope = this.Services.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        await dbCtx.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = this.Services.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        // Truncate all application tables and restart identities
        // Order doesn't matter with CASCADE
        var sql = @"TRUNCATE TABLE
            org_members,
            chat_messages,
            chat_threads,
            folders,
            users,
            orgs
            RESTART IDENTITY CASCADE;";
        await dbCtx.Database.ExecuteSqlRawAsync(sql);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await this.pg.StopAsync();
        await this.pg.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(
            services =>
        {
        });
    }
}
