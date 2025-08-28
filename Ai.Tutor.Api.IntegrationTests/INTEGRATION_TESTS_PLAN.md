# Integration Testing Architecture Plan

This document describes the target test architecture for `Ai.Tutor.Api.IntegrationTests/`, adopting stable patterns: a shared collection fixture, a common test base class, database reset via Respawn, and deterministic, non-parallel execution to avoid cross-test interference.

## Goals

- Ensure fast, reliable, isolated integration tests.
- Reuse test bootstrapping logic across test classes.
- Keep application wiring in `ai-tutor-api/Program.cs` (no bespoke DI per test) by using configuration/env vars.
- Avoid parallelization-induced flakiness when using a shared PostgreSQL container and schema.

## Current state (recap)

- Container lifecycle is inside `TestWebAppFactory` (`Testcontainers.PostgreSql`).
- Connection string is provided through `ConnectionStrings__Default` env var.
- Tests seed data directly via `Helpers/DbSeed.cs`.
- Individual test classes create their own client and scopes.

## Proposed architecture

1. __xUnit Collection + Collection Fixture__
   - Create a single collection (e.g., `ApiIntegrationTests`) that:
     - Starts/stops the PostgreSQL Testcontainer once per test run.
     - Sets `ConnectionStrings__Default` for the app under test.
     - Creates and holds a single `WebApplicationFactory<Program>` instance for reuse.
   - Disable parallelization for this collection only to avoid concurrent DB interactions.

2. __FactoryFixture__ (shared across all tests)
   - Exposes:
     - `Factory` (WebApplicationFactory<Program>)
     - `Services` (IServiceProvider)
     - `CreateClient()` helper
     - `ConnectionString` (container-generated)
     - `ResetDatabaseAsync()` using Respawn (see below)
   - Manages DB migrations once after startup.

3. __Database reset with Respawn__
   - Add `Respawn` NuGet package to the test project.
   - Configure a `Checkpoint` for PostgreSQL (Npgsql) to wipe data between tests while preserving schema/migrations:
     - Ignore tables like `__EFMigrationsHistory`.
     - Include correct schema (usually `public`).
   - Implement `ResetDatabaseAsync()` that opens an Npgsql connection to the test DB and calls `checkpoint.Reset(...)`.

4. __Common base class: `IntegrationTestBase`__
   - `abstract class IntegrationTestBase : IClassFixture<FactoryFixture>, IAsyncLifetime`
   - Provides:
     - `Factory` and `Services` accessors
     - `CreateClient()` method
     - `CreateScope()` helper for resolving scoped services (e.g., `AiTutorDbContext`)
     - Hooks `InitializeAsync`/`DisposeAsync` to call `ResetDatabaseAsync()` before each test (and optionally after).
   - All integration test classes inherit from this base.

5. __Seeding helpers__
   - Keep and extend `Helpers/DbSeed.cs` (or move to `TestData/DbSeed.cs`).
   - Ensure all test data setup uses these helpers for consistency.
   - Patterns: return created entities; avoid hard-coded dates; default to `DateTime.UtcNow`.

6. __Parallelization__
   - Disable parallelization for the integration collection using `[CollectionDefinition("ApiIntegrationTests", DisableParallelization = true)]`.
   - Keep unit tests (if any) parallelized.

7. __Determinism & reliability__
   - Use UTC; never `DateTime.Now`.
   - Ensure stable ordering in queries/assertions (e.g., explicit `OrderBy` in repo layer if relevant to test assertions).
   - Set generous but bounded timeouts for HTTP calls if needed.
   - Consider capturing logs for diagnostics (Serilog sink to test output) when failures occur.

8. __Configuration & env vars__
   - Continue using env vars for container and connection string so `Program.cs` stays authoritative:
     - `ITESTS_PG_IMAGE` (default `postgres:17-alpine`)
     - `ITESTS_PG_DB` (default `ai_tutor_test`)
     - `ITESTS_PG_USER` (default `postgres`)
     - `ITESTS_PG_PASSWORD` (default `postgres`)
     - `ConnectionStrings__Default` (set to container connection string)

9. __File and type organization__
   - One type per file (matches repo preference):
     - `TestCollection.cs` (CollectionDefinition + empty marker class)
     - `FactoryFixture.cs` (collection fixture)
     - `IntegrationTestBase.cs` (abstract)
     - `DatabaseReset.cs` (Respawn logic)
     - Keep `Helpers/DbSeed.cs` or move to `TestData/DbSeed.cs`

## Refactor steps (phased)

Phase 1 – Infrastructure
- Add NuGet package: `Respawn`.
- Create files:
  - `TestCollection.cs` with `[CollectionDefinition("ApiIntegrationTests", DisableParallelization = true)]`.
  - `FactoryFixture.cs` implementing `IAsyncLifetime`:
    - Start Testcontainers PG once.
    - Set `ConnectionStrings__Default`.
    - Create `WebApplicationFactory<Program>` and run migrations.
    - Hold `Checkpoint` and implement `ResetDatabaseAsync()`.
  - `IntegrationTestBase.cs` providing shared helpers and invoking `ResetDatabaseAsync()` in `InitializeAsync()`.

Phase 2 – Migrate existing tests
- Update `FoldersEndpointsTests` and `ThreadsEndpointsTests` to:
  - Decorate with `[Collection("ApiIntegrationTests")]`.
  - Inherit `IntegrationTestBase`.
  - Replace explicit factory/scope creation with base helpers (`CreateClient()`, `CreateScope()`).
  - Remove per-test migrations/bootstrapping.

Phase 3 – Seeding and utilities
- Centralize additional seed helpers if needed (e.g., composite seed workflows).
- Add optional HttpClient/test utilities (e.g., JSON helpers, retry-on-409 if needed).

Phase 4 – Documentation and CI
- Document env vars, Docker prerequisites, and how to run tests locally/CI.
- In CI, ensure Docker is available and the `postgres` image is accessible; set any env overrides as needed.

## Example skeletons (illustrative)

> Note: below are conceptual outlines; actual code will be added in the refactor PR after your approval.

- Collection definition:
```csharp
[CollectionDefinition("ApiIntegrationTests", DisableParallelization = true)]
public sealed class ApiIntegrationTestsCollection : ICollectionFixture<FactoryFixture>
{
}
```

- Factory fixture (key responsibilities):
```csharp
public sealed class FactoryFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; } = default!;
    public IServiceProvider Services => Factory.Services;
    public string ConnectionString { get; private set; } = string.Empty;

    private PostgreSqlContainer _pg = default!;
    private Checkpoint _checkpoint = default!;

    public async Task InitializeAsync()
    {
        // Start container, set ConnectionStrings__Default, build Factory, run migrations, create Respawn checkpoint.
    }

    public async Task ResetDatabaseAsync()
    {
        // Use NpgsqlConnection + _checkpoint.Reset(conn)
    }

    public async Task DisposeAsync()
    {
        // Dispose factory and stop container
    }
}
```

- Base class:
```csharp
public abstract class IntegrationTestBase : IClassFixture<FactoryFixture>, IAsyncLifetime
{
    protected IntegrationTestBase(FactoryFixture fixture) { /* save fixture */ }
    protected HttpClient CreateClient() => /* fixture.Factory.CreateClient() */;
    protected IServiceScope CreateScope() => /* fixture.Services.CreateScope() */;

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;
}
```

## Acceptance checklist

- Single PG container per test run, reused by all tests.
- Connection string via configuration (no DI overrides).
- Database cleaned between tests with Respawn.
- Non-parallel integration test collection.
- Tests inherit a common base and use shared seeding helpers.
- One type per file across new test infrastructure.

---

If you approve this plan, I’ll implement it in small PR-sized steps: Phase 1 (infrastructure), Phase 2 (migrate tests), then Phase 3/4 (polish and docs).
