namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using Contracts.DTOs;
using Contracts.Enums;
using FluentAssertions;
using Helpers;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class ThreadsEndpointsTests : IntegrationTestBase, IClassFixture<TestWebAppFactory>
{
    public ThreadsEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Get_Update_List_Delete_Thread_Succeeds()
    {
        var client = this.Factory.CreateClient();

        // Seed baseline Org & User (& optional Folder)
        using var scope = this.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var folder = await DbSeed.EnsureFolderAsync(db, org.Id, user.Id);

        // Create
        var createReq = new CreateThreadRequest
        {
            UserId = user.Id,
            FolderId = folder.Id,
            Title = "My first thread",
            Status = ThreadStatus.Active,
            SortOrder = 1000m,
        };

        var createResp = await client.PostAsJsonAsync($"/api/orgs/{org.Id}/threads", createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ThreadDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("My first thread");
        created.UserId.Should().Be(user.Id);
        created.FolderId.Should().Be(folder.Id);

        // Get by id
        var getResp = await client.GetAsync(new Uri($"/api/orgs/{org.Id}/threads/{created.Id}"));
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResp.Content.ReadFromJsonAsync<ThreadDto>();
        fetched!.Id.Should().Be(created.Id);

        // List by folder
        var listByFolder = await client.GetFromJsonAsync<List<ThreadDto>>(new Uri($"/api/orgs/{org.Id}/threads/by-folder?folderId={folder.Id}"));
        listByFolder.Should().NotBeNull();
        listByFolder!.Any(t => t.Id == created.Id).Should().BeTrue();

        // List by user
        var listByUser = await client.GetFromJsonAsync<List<ThreadDto>>(new Uri($"/api/orgs/{org.Id}/threads/by-user/{user.Id}"));
        listByUser.Should().NotBeNull();
        listByUser!.Any(t => t.Id == created.Id).Should().BeTrue();

        // Update
        var updateReq = new UpdateThreadRequest
        {
            Title = "Updated title",
            Status = ThreadStatus.Archived,
            SortOrder = 999m,
        };
        var updateResp = await client.PatchAsJsonAsync(new Uri($"/api/orgs/{org.Id}/threads/{created.Id}"), updateReq);
        updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update via GET
        var afterUpdate = await client.GetFromJsonAsync<ThreadDto>(new Uri($"/api/orgs/{org.Id}/threads/{created.Id}"));
        afterUpdate!.Title.Should().Be("Updated title");
        afterUpdate.Status.Should().Be(ThreadStatus.Archived);
        afterUpdate.SortOrder.Should().Be(999m);

        // Delete
        var deleteResp = await client.DeleteAsync(new Uri($"/api/orgs/{org.Id}/threads/{created.Id}"));
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify 404
        var getAfterDelete = await client.GetAsync(new Uri($"/api/orgs/{org.Id}/threads/{created.Id}"));
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
