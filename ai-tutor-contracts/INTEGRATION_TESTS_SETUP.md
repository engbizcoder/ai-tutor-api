# Integration Tests Setup Guide (C#/.NET, following sericy-mfopt-line-api)

## Index
- [Overview](#overview)
- [Project structure](#project-structure)
- [Dependencies](#dependencies)
- [Environment configuration in the app](#environment-configuration-in-the-app)
- [Testcontainers setup](#testcontainers-setup)
- [WebApplicationFactory configuration](#webapplicationfactory-configuration)
- [Database migrations strategy (once for all tests)](#database-migrations-strategy-once-for-all-tests)
- [Deterministic readiness checks](#deterministic-readiness-checks)
- [Database cleanup with Respawn (per test)](#database-cleanup-with-respawn-per-test)
- [Base test class](#base-test-class)
- [Writing a test](#writing-a-test)
- [Avoid rebuilding the host per test (DI overrides)](#avoid-rebuilding-the-host-per-test-di-overrides)
- [Running tests (local and CI)](#running-tests-local-and-ci)
- [Quick checklist](#quick-checklist)
- [Appendix: sample files to copy](#appendix-sample-files-to-copy)

---

## Overview
- Boot the real API with `WebApplicationFactory<Program>`.
- Use Testcontainers for real infra: PostgreSQL, Redis, RabbitMQ.
- Reuse production DI and `DbContext` registration; override only what’s needed for tests.
- Run EF Core migrations exactly once for the whole test assembly.
- Add readiness waits and an optional first-connection retry to avoid flakiness.
- Use Respawn to reset DB state per test instead of re-running migrations.

References in current repo for orientation:
- `sericy-mfopt-line-api-integration-tests/Utils/ApiApplicationFactory.cs`
- `sericy-mfopt-line-services/Extensions/ServiceCollectionExtensions.cs`
- `sericy-mfopt-line-repository/DataAccess/DbContextRegistrationExtensions.cs`
- `sericy-mfopt-line-api/Program.cs`

---

## Project structure
- your-app-integration-tests/
  - Utils/
    - ApiApplicationFactory.cs
    - IntegrationTestsBase.cs
    - TestAuthHandler.cs (if needed)
    - TestClaimsProvider.cs (if needed)
    - OverriddenApiApplicationFactory.cs (optional helper)
    - DbSchemaFixture.cs (one-time migrations in tests)
    - RespawnCheckpoint.cs (shared DB reset helper)
  - <FeatureName>/
    - <ControllerOrFlow>Tests.cs
  - AdvancedConfigurationJsonFiles/ (optional)

Why: mirrors this repo; keeps bootstrapping reusable and tests focused.

---

## Dependencies
NuGet packages:
- Microsoft.AspNetCore.Mvc.Testing
- xunit, xunit.runner.visualstudio (or coverlet.collector)
- DotNet.Testcontainers (PostgreSql, Redis, RabbitMq)
- Microsoft.EntityFrameworkCore.Design, Npgsql.EntityFrameworkCore.PostgreSQL
- Serilog packages used by the app
- Respawn (for DB cleanup)
- Polly (optional, for retry around first DB use)

---

## Environment configuration in the app
Your main app should:
- Load configuration from JSON + environment variables.
- Register services via a central extension (e.g., `ServiceCollectionExtensions.AddServices(...)`), which calls `RegisterDbContext(...)`.
- Register DbContext using `UseNpgsql(connectionString)` from `ConnectionStrings__<YourContextName>`.
- Gate migrations with a flag so tests can disable them:

```csharp
// Program.cs (after building app)
var runMigrations = app.Configuration.GetValue<bool>("RunMigrationsOnStartup", true);
if (runMigrations)
{
    app.RunMigrations(); // calls db.Database.Migrate()
}
```

---

## Testcontainers setup
Create containers and set env vars your app expects.

Example snippet for `Utils/ApiApplicationFactory.cs`:

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Containers.WaitStrategies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

public class ApiApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql = new PostgreSqlBuilder()
        .WithImage("docker-registry.grenzebach.com/postgres:17.2-alpine")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        // Deterministic readiness
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        // If available in the image: .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres"))
        .Build();

    private readonly RedisContainer redis = new RedisBuilder()
        .WithImage("docker-registry.grenzebach.com/redis:7.4.1")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    private readonly RabbitMqContainer rabbitMq = new RabbitMqBuilder()
        .WithImage("docker-registry.grenzebach.com/rabbitmq:4.0.2")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
        .Build();

    public async Task InitializeAsync()
    {
        await redis.StartAsync();
        await rabbitMq.StartAsync();
        await postgreSql.StartAsync();

        Set("ConnectionStrings__YourContext", postgreSql.GetConnectionString());
        Set("ConnectionStrings__Redis", $"{redis.GetConnectionString()},allowAdmin=true, abortConnect=false");
        Set("RabbitMq__VirtualHost", "/");
        Set("RabbitMq__HostName", rabbitMq.Hostname);
        Set("RabbitMq__UserName", RabbitMqBuilder.DefaultUsername);
        Set("RabbitMq__Password", RabbitMqBuilder.DefaultPassword);
        Set("RabbitMq__Port", rabbitMq.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort).ToString());

        // Disable automatic migrations in tests; we will run them once via assembly fixture
        Set("RunMigrationsOnStartup", "false");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await redis.DisposeAsync();
        await rabbitMq.DisposeAsync();
        await postgreSql.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            // Add test-only auth/options/mocks if needed
        });
    }

    private static void Set(string key, string value) =>
        Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
}
```

---

## WebApplicationFactory configuration
- Force `Testing` environment.
- Override only minimal services (auth/options/mocks).
- Keep production DI path intact.

---

## Database migrations strategy (once for all tests)
Goal: run migrations only once for the entire test run.

1) Gate in `Program.cs` (see above).
2) Disable in tests by default: set `RunMigrationsOnStartup=false` in `ApiApplicationFactory.InitializeAsync()`.
3) Create an assembly fixture that runs migrations one time:

```csharp
// Utils/DbSchemaFixture.cs
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Xunit;

public class DbSchemaFixture : IClassFixture<ApiApplicationFactory>, IAsyncLifetime
{
    private readonly ApiApplicationFactory _factory;
    public DbSchemaFixture(ApiApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        // Short retry for rare first-connection jitter
        AsyncRetryPolicy retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, attempt => TimeSpan.FromMilliseconds(200 * attempt));

        await retry.ExecuteAsync(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<YourDbContext>();
            await db.Database.MigrateAsync();
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

// Utils/GlobalFixtures.cs
using Xunit;

// xUnit will create this ONCE for the entire test assembly
public class GlobalDbSchemaAssemblyFixture : IAssemblyFixture<DbSchemaFixture> { }
```

This ensures migrations run exactly once per test assembly.

---

## Deterministic readiness checks
Even with migrations, container start ≠ service ready.

- Testcontainers waits (already added above):
  - Postgres: `UntilPortIsAvailable(5432)` or `pg_isready` if available.
  - Redis: `UntilPortIsAvailable(6379)`.
  - RabbitMQ: `UntilPortIsAvailable(5672)`.
- First-connection retry (in `DbSchemaFixture`) covers rare startup jitter.

Why: prevents intermittent “connection refused” on first DB use.

---

## Database cleanup with Respawn (per test)
Use Respawn to reset the DB between tests without re-running migrations.

```csharp
// Utils/RespawnCheckpoint.cs
using Npgsql;
using Respawn;

public static class RespawnCheckpoint
{
    private static readonly Checkpoint Checkpoint = new()
    {
        DbAdapter = DbAdapter.Postgres,
        SchemasToInclude = new[] { "public" },
        TablesToIgnore = new[] { "__EFMigrationsHistory" },
        // WithReseed = true // optional
    };

    public static async Task ResetAsync(string connectionString, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await Checkpoint.Reset(conn);
    }
}
```

Call Respawn per test in your base class:

```csharp
// Utils/IntegrationTestsBase.cs (variant with Respawn)
using Xunit;
using Xunit.Abstractions;

public abstract class IntegrationTestsBase : IClassFixture<ApiApplicationFactory>, IAsyncLifetime
{
    protected IntegrationTestsBase(ITestOutputHelper output, ApiApplicationFactory factory)
    {
        Factory = factory;
        // logging/culture setup omitted for brevity
    }

    protected ApiApplicationFactory Factory { get; }

    public async Task InitializeAsync()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__YourContext");
        if (!string.IsNullOrEmpty(cs))
        {
            await RespawnCheckpoint.ResetAsync(cs);
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

---

## Base test class
Common helpers, logging, culture, and optional Respawn call (as shown above). Example without Respawn for brevity:

```csharp
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

public abstract class IntegrationTestsBase : IClassFixture<ApiApplicationFactory>
{
    protected IntegrationTestsBase(ITestOutputHelper output, ApiApplicationFactory factory)
    {
        Factory = factory;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output,
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
            .CreateLogger();

        var culture = CultureInfo.CreateSpecificCulture("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    protected ApiApplicationFactory Factory { get; }

    protected T GetService<T>() where T : notnull =>
        Factory.Services.GetRequiredService<T>();
}
```

---

## Writing a test
Example using shared client with schema migrated once by the assembly fixture:

```csharp
using System.Net;
using Xunit;
using Xunit.Abstractions;

public class ExampleControllerTests : IntegrationTestsBase
{
    public ExampleControllerTests(ITestOutputHelper output, ApiApplicationFactory factory)
        : base(output, factory) { }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = Factory.CreateClient();
        var resp = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
```

---

## Avoid rebuilding the host per test (DI overrides)
Only use `WithWebHostBuilder(...)` when a single test needs unique overrides. For recurring overrides, prefer per-class custom factories so the host (and migrations) start once per class.

A) Delegate-based factory:
```csharp
public class OverriddenApiApplicationFactory : ApiApplicationFactory
{
    private readonly Action<IServiceCollection>? _configure;
    public OverriddenApiApplicationFactory(Action<IServiceCollection>? configure = null) => _configure = configure;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        if (_configure is null) return;
        builder.ConfigureTestServices(s => _configure(s));
    }
}
```

B) Subclass per feature with fixed overrides.

Benefit: no per-test host rebuilds; fewer redundant migrations.

---

## Running tests (local and CI)
- Local: ensure Docker Desktop is running; `dotnet test`.
- CI: agent must have Docker; Testcontainers will pull and run images.
- Cache Docker layers in CI to speed up pulls.
- If you parallelize, ensure port/data isolation (or run collections sequentially).

---

## Quick checklist
- App:
  - `Program.cs`: config, DI, `RegisterDbContext`, and a `RunMigrationsOnStartup` gate.
  - `DbContextRegistrationExtensions`: `UseNpgsql` from `ConnectionStrings__YourContext`.
- Tests:
  - `ApiApplicationFactory`: starts Testcontainers, sets env vars, adds wait strategies, sets `RunMigrationsOnStartup=false`.
  - `DbSchemaFixture` + `GlobalDbSchemaAssemblyFixture`: run migrations once for the whole assembly.
  - Respawn: truncate DB per test if clean state is needed.
  - `IntegrationTestsBase`: common logging/culture and optional Respawn hook.
  - Prefer per-class custom factories for recurring DI overrides.

---

## Appendix: sample files to copy

1) `Utils/ApiApplicationFactory.cs`
- Combine the Testcontainers setup, readiness waits, and set `RunMigrationsOnStartup=false`.

2) `Utils/DbSchemaFixture.cs`
- One-time migrations per assembly with a short retry.

3) `Utils/GlobalFixtures.cs`
- Registers the assembly fixture.

4) `Utils/RespawnCheckpoint.cs`
- Shared Respawn checkpoint helper.

5) `Utils/IntegrationTestsBase.cs`
- Common base with optional Respawn reset.

6) `Utils/OverriddenApiApplicationFactory.cs` (optional)
- For per-class DI overrides without per-test host rebuild.

7) Example test
```csharp
public class ExampleControllerTests : IntegrationTestsBase
{
    public ExampleControllerTests(ITestOutputHelper output, ApiApplicationFactory factory) : base(output, factory) { }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = Factory.CreateClient();
        var resp = await client.GetAsync("/health");
        Assert.True(resp.IsSuccessStatusCode);
    }
}
```
