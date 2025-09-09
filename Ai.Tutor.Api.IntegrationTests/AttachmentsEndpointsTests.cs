namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ai.Tutor.Api.IntegrationTests.Helpers;
using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Infrastructure.Data;
using Contracts.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class AttachmentsEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public AttachmentsEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_List_GetById_Attachment_Succeeds()
    {
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);
        var message = await DbSeed.EnsureMessageAsync(db, thread.Id);
        var file = await UploadSmallFileAsync(client, org.Id, user.Id);

        // Create attachment
        var createReq = new CreateAttachmentRequest
        {
            FileId = file.Id,
            Type = AttachmentType.Document,
        };
        var createResponse = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages/{message.Id}/attachments"), createReq);
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<AttachmentDto>(createBody, Options)!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(message.Id, created.MessageId);
        Assert.Equal(file.Id, created.FileId);

        // List
        var listResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages/{message.Id}/attachments"));
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<AttachmentDto>>(listBody, Options)!;
        Assert.Contains(list, a => a.Id == created.Id);

        // Get by id
        var getResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages/{message.Id}/attachments/{created.Id}"));
        getResponse.EnsureSuccessStatusCode();
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var got = JsonSerializer.Deserialize<AttachmentDto>(getBody, Options)!;
        Assert.Equal(created.Id, got.Id);
    }

    private static async Task<FileDto> UploadSmallFileAsync(HttpClient client, Guid orgId, Guid ownerUserId)
    {
        var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("attach file");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, name: "file", fileName: "attach.txt");
        content.Add(new StringContent("attach.txt"), "FileName");
        content.Add(new StringContent("text/plain"), "ContentType");

        var uploadResponse = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/orgs/{orgId}/files?ownerUserId={ownerUserId}"), content);
        uploadResponse.EnsureSuccessStatusCode();
        var body = await uploadResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileDto>(body, Options)!;
    }
}
