namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ai.Tutor.Api.IntegrationTests.Helpers;
using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class FilesEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public FilesEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Upload_And_GetById_And_List_Succeeds()
    {
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);

        // Build multipart form data: meta fields + file
        using var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("hello world");
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, name: "file", fileName: "hello.txt");

        using var fileNameContent = new StringContent("hello.txt");
        content.Add(fileNameContent, "FileName");

        using var contentTypeContent = new StringContent("text/plain");
        content.Add(contentTypeContent, "ContentType");

        using var uploadResponse = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/files?ownerUserId={user.Id}"), content);
        uploadResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);

        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        var fileDto = JsonSerializer.Deserialize<FileDto>(uploadBody, Options)!;
        Assert.NotEqual(Guid.Empty, fileDto.Id);
        Assert.Equal("hello.txt", fileDto.FileName);
        Assert.Equal(user.Id, fileDto.OwnerUserId);

        // GET by id
        var getResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/files/{fileDto.Id}"));
        getResponse.EnsureSuccessStatusCode();
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var got = JsonSerializer.Deserialize<FileDto>(getBody, Options);
        Assert.NotNull(got);
        Assert.Equal(fileDto.Id, got!.Id);

        // LIST
        var listResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/files?ownerUserId={user.Id}&pageSize=10"));
        listResponse.EnsureSuccessStatusCode();
        var listBody = await listResponse.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<PagedFilesResponse>(listBody, Options);
        Assert.NotNull(list);
        Assert.NotEmpty(list!.Items);
        Assert.Contains(list.Items, x => x.Id == fileDto.Id);
    }

    [Fact]
    public async Task Download_ReturnsStreamWithHeaders()
    {
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);

        // Upload a small file first
        using var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes("download me");
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, name: "file", fileName: "readme.txt");

        using var fileNameContent = new StringContent("readme.txt");
        content.Add(fileNameContent, "FileName");

        using var contentTypeContent = new StringContent("text/plain");
        content.Add(contentTypeContent, "ContentType");

        using var uploadResponse = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/files?ownerUserId={user.Id}"), content);
        uploadResponse.EnsureSuccessStatusCode();
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        var fileDto = JsonSerializer.Deserialize<FileDto>(uploadBody, Options)!;

        // Download
        using var downloadResponse = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/files/{fileDto.Id}/download"));
        downloadResponse.EnsureSuccessStatusCode();
        using var downloadContent = downloadResponse.Content;
        Assert.Equal("text/plain", downloadContent.Headers.ContentType?.MediaType);
        var data = await downloadContent.ReadAsByteArrayAsync();
        Assert.Equal(fileBytes, data);
    }
}
