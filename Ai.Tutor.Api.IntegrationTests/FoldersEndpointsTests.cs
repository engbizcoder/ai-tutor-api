namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using FluentAssertions;
using Helpers;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class FoldersEndpointsTests : IntegrationTestBase
{
    public FoldersEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Delete_Folder_Cascades_To_Threads_And_Messages()
    {
        var client = this.CreateClient();

        // Seed full hierarchy: org, user, folder, thread
        var (org, user, folder, thread) = await this.SeedFullHierarchyAsync();

        // Add a message to the thread
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        await DbSeed.EnsureMessageAsync(db, thread.Id);

        // Act: delete folder
        var resp = await client.DeleteAsync(new Uri(client.BaseAddress, $"/api/orgs/{org.Id}/folders/{folder.Id}"));
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: thread is gone
        var getThread = await client.GetAsync(new Uri(client.BaseAddress, $"/api/orgs/{org.Id}/threads/{thread.Id}"));
        getThread.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
