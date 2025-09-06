namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Infrastructure.Data;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class MessagesEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public MessagesEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListMessages_ValidRequest_ReturnsPagedMessages()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);
        var message = await DbSeed.EnsureMessageAsync(db, thread.Id);

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages?pageSize=10"));

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedMessagesResponse>(content, Options);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(message.Id, result.Items[0].Id);
        Assert.Equal(thread.Id, result.Items[0].ThreadId);
    }

    [Fact]
    public async Task ListMessages_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages?pageSize=0"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("MESSAGES_001", content);
    }

    [Fact]
    public async Task ListMessages_NonExistentThread_ReturnsNotFound()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, _) = await DbSeed.EnsureOrgAndUserAsync(db);
        var nonExistentThreadId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{nonExistentThreadId}/messages"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMessage_ValidRequest_ReturnsCreatedMessage()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);

        var request = new CreateMessageRequest
        {
            Content = "Test message content",
            SenderType = Contracts.Enums.SenderType.User,
            SenderId = user.Id,
            MetadataJson = "{\"test\": \"value\"}",
            IdempotencyKey = Guid.NewGuid().ToString(),
        };

        // Act
        var response = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages"), request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MessageDto>(content, Options);

        Assert.NotNull(result);
        Assert.Equal(request.Content, result.Content);
        Assert.Equal(thread.Id, result.ThreadId);
        Assert.Equal(user.Id, result.SenderId);
    }

    [Fact]
    public async Task CreateMessage_InvalidContent_ReturnsBadRequest()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);

        var request = new CreateMessageRequest
        {
            Content = string.Empty, // Invalid empty content
            SenderType = Contracts.Enums.SenderType.User,
            SenderId = user.Id,
        };

        // Act
        var response = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages"), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Message content is required", content); // Content validation error message
    }

    [Fact]
    public async Task CreateMessage_NonExistentThread_ReturnsNotFound()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var nonExistentThreadId = Guid.NewGuid();

        var request = new CreateMessageRequest
        {
            Content = "Test message content",
            SenderType = Contracts.Enums.SenderType.User,
            SenderId = user.Id,
        };

        // Act
        var response = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{nonExistentThreadId}/messages"), request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMessage_WithIdempotencyKey_PreventsDuplicates()
    {
        // Arrange
        using var client = this.CreateClient();
        using var scope = this.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var (org, user) = await DbSeed.EnsureOrgAndUserAsync(db);
        var thread = await DbSeed.EnsureThreadAsync(db, org.Id, user.Id, null);

        var idempotencyKey = Guid.NewGuid().ToString();
        var request = new CreateMessageRequest
        {
            Content = "Test message content",
            SenderType = Contracts.Enums.SenderType.User,
            SenderId = user.Id,
            IdempotencyKey = idempotencyKey,
        };

        // Act - First request
        var response1 = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages"), request);

        // Act - Second request with same idempotency key
        var response2 = await client.PostAsJsonAsync(new Uri(client.BaseAddress!, $"/api/orgs/{org.Id}/threads/{thread.Id}/messages"), request);

        // Assert
        response1.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        // Second request should return the same message (idempotent behavior)
        response2.EnsureSuccessStatusCode();

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();
        var result1 = JsonSerializer.Deserialize<MessageDto>(
            content1,
            Options);
        var result2 = JsonSerializer.Deserialize<MessageDto>(content2, Options);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Id, result2.Id); // Same message returned
    }
}
