namespace Ai.Tutor.Api.IntegrationTests;

using System.Net;
using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Infrastructure.Data;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("Database Integration Tests")]
public sealed class OrganizationsEndpointsTests : IntegrationTestBase
{
    public OrganizationsEndpointsTests(TestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DisableOrganization_ValidOrgId_ReturnsNoContent()
    {
        // Arrange
        using var scope = this.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var org = await DbSeed.CreateOrgAsync(dbContext, "Test Org", "test-org");

        // Act
        var client = this.CreateClient();
        var response = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}/disable"), null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify org is disabled - use fresh scope to avoid stale data
        using var verifyScope = this.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var updatedOrg = await verifyDbContext.Orgs.FindAsync(org.Id);
        Assert.NotNull(updatedOrg);
        Assert.Equal(OrgLifecycleStatus.Disabled, updatedOrg.LifecycleStatus);
        Assert.NotNull(updatedOrg.DisabledAt);
    }

    [Fact]
    public async Task DisableOrganization_NonExistentOrgId_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOrgId = Guid.NewGuid();

        // Act
        var client = this.CreateClient();
        var response = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/organizations/{nonExistentOrgId}/disable"), null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SoftDeleteOrganization_ValidOrgId_ReturnsNoContent()
    {
        // Arrange
        using var scope = this.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var org = await DbSeed.CreateOrgAsync(dbContext, "Test Org", "test-org");

        // Act
        var client = this.CreateClient();
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}"));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify org is soft-deleted - use fresh scope to avoid stale data
        using var verifyScope = this.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var updatedOrg = await verifyDbContext.Orgs.FindAsync(org.Id);
        Assert.NotNull(updatedOrg);
        Assert.Equal(OrgLifecycleStatus.Deleted, updatedOrg.LifecycleStatus);
        Assert.NotNull(updatedOrg.DeletedAt);
        Assert.NotNull(updatedOrg.PurgeScheduledAt);
    }

    [Fact]
    public async Task SoftDeleteOrganization_NonExistentOrgId_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOrgId = Guid.NewGuid();

        // Act
        var client = this.CreateClient();
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/organizations/{nonExistentOrgId}"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HardDeleteOrganization_ValidOrgId_ReturnsNoContent()
    {
        // Arrange
        using var scope = this.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var org = await DbSeed.CreateOrgAsync(dbContext, "Test Org", "test-org");

        var client = this.CreateClient();

        // First soft-delete the organization (required before hard delete)
        var softDeleteResponse = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}"));
        Assert.Equal(HttpStatusCode.NoContent, softDeleteResponse.StatusCode);

        // Update the org to have an expired retention period so it can be hard-deleted
        using var updateScope = this.CreateScope();
        var updateDbContext = updateScope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var orgToUpdate = await updateDbContext.Orgs.FindAsync(org.Id);
        Assert.NotNull(orgToUpdate);
        orgToUpdate.PurgeScheduledAt = DateTime.UtcNow.AddDays(-1); // Set to past date
        await updateDbContext.SaveChangesAsync();

        // Act - Hard delete the organization
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}/hard"));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify org is hard-deleted (no longer exists) - use fresh scope to avoid stale data
        using var verifyScope = this.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AiTutorDbContext>();
        var deletedOrg = await verifyDbContext.Orgs.FindAsync(org.Id);
        Assert.Null(deletedOrg);
    }

    [Fact]
    public async Task HardDeleteOrganization_NonExistentOrgId_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOrgId = Guid.NewGuid();

        // Act
        var client = this.CreateClient();
        var response = await client.DeleteAsync(new Uri(client.BaseAddress!, $"/api/organizations/{nonExistentOrgId}/hard"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DisableOrganization_AlreadyDisabledOrg_ReturnsBadRequest()
    {
        // Arrange
        using var scope = this.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiTutorDbContext>();

        var org = await DbSeed.CreateOrgAsync(dbContext, "Test Org", "test-org");

        var client = this.CreateClient();

        // First disable the org
        await client.PostAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}/disable"), null);

        // Act - Try to disable again
        var response = await client.PostAsync(new Uri(client.BaseAddress!, $"/api/organizations/{org.Id}/disable"), null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
