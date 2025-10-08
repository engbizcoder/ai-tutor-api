namespace Ai.Tutor.Api.IntegrationTests;

using System.Threading.Tasks;
using Helpers;
using Infrastructure.Data;
using Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public abstract class IntegrationTestBase : IAsyncLifetime
{
#pragma warning disable CA1051
    protected readonly TestWebAppFactory Factory;
#pragma warning restore CA1051

    protected IntegrationTestBase(TestWebAppFactory factory)
    {
        this.Factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Reset DB before each test
        await this.Factory.ResetDatabaseAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        // Optionally reset after each test to keep a clean slate
        await this.Factory.ResetDatabaseAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an HTTP client for making requests to the test server.
    /// </summary>
    protected HttpClient CreateClient() => this.Factory.CreateClient();

    /// <summary>
    /// Creates a service scope for accessing services directly.
    /// </summary>
    protected IServiceScope CreateScope() => this.Factory.Services.CreateScope();

    /// <summary>
    /// Gets a required service from the DI container.
    /// </summary>
    protected T GetService<T>()
        where T : notnull
        => this.Factory.Services.GetRequiredService<T>();

    /// <summary>
    /// Gets the database context from a new scope.
    /// </summary>
    protected AiTutorDbContext GetDbContext()
    {
        using var scope = this.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
    }

    /// <summary>
    /// Seeds baseline organization and user data for tests.
    /// </summary>
    protected async Task<(OrgRecord Org, UserRecord User)> SeedOrgAndUserAsync()
    {
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        return await DbSeed.EnsureOrgAndUserAsync(db).ConfigureAwait(false);
    }

    /// <summary>
    /// Seeds baseline organization, user, and folder data for tests.
    /// </summary>
    protected async Task<(OrgRecord Org, UserRecord User, FolderRecord Folder)> SeedOrgUserAndFolderAsync()
    {
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db).ConfigureAwait(false);
        var folder = await DbSeed.EnsureFolderAsync(db, org.Id, user.Id).ConfigureAwait(false);
        return (org, user, folder);
    }

    /// <summary>
    /// Seeds baseline organization, user, folder, and thread data for tests.
    /// </summary>
    protected async Task<(OrgRecord Org, UserRecord User, FolderRecord Folder, ThreadRecord Thread)> SeedFullHierarchyAsync()
    {
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db).ConfigureAwait(false);
        var folder = await DbSeed.EnsureFolderAsync(db, org.Id, user.Id).ConfigureAwait(false);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, folder.Id).ConfigureAwait(false);
        return (org, user, folder, thread);
    }
}
