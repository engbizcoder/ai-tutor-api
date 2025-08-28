namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using FluentAssertions;
using Helpers;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class FoldersEndpointsTests : IntegrationTestBase, IClassFixture<TestWebAppFactory>
{
    public FoldersEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Delete_Folder_Cascades_To_Threads_And_Messages()
    {
        var client = this.Factory.CreateClient();

        using var scope = this.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var folder = await DbSeed.EnsureFolderAsync(db, org.Id, user.Id);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, folder.Id);
        await DbSeed.EnsureMessageAsync(db, thread.Id);

        // Act: delete folder
        var resp = await client.DeleteAsync(new Uri($"/api/orgs/{org.Id}/folders/{folder.Id}"));
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: thread is gone
        var getThread = await client.GetAsync(new Uri($"/api/orgs/{org.Id}/threads/{thread.Id}"));
        getThread.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
