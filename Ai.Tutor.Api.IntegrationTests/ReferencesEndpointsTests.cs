namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ai.Tutor.Api.IntegrationTests.Helpers;
using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Contracts.Enums;
using Ai.Tutor.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class ReferencesEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public ReferencesEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_List_GetById_Reference_Succeeds()
    {
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);
        var message = await DbSeed.EnsureMessageAsync(db, thread.Id);

        var req = new CreateReferenceRequest
        {
            MessageId = message.Id,
            Type = ReferenceType.Link,
            Title = "External Resource",
            Url = "https://example.com/resource",
        };

        var createResponse = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/references"), req);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ReferenceDto>(createBody, Options)!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(thread.Id, created.ThreadId);

        // List
        var listResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/references"));
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<PagedReferencesResponse>(listBody, Options)!;
        Assert.Contains(list.Items, r => r.Id == created.Id);

        // Get by id
        var getResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/references/{created.Id}"));
        getResponse.EnsureSuccessStatusCode();
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var got = JsonSerializer.Deserialize<ReferenceDto>(getBody, Options)!;
        Assert.Equal(created.Id, got.Id);
    }
}
