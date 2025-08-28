namespace Ai.Tutor.Api.IntegrationTests;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebAppFactory Factory;

    protected IntegrationTestBase(TestWebAppFactory factory)
    {
        this.Factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Reset DB before each test
        await this.Factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // Optionally reset after each test to keep a clean slate
        await this.Factory.ResetDatabaseAsync();
    }
}
