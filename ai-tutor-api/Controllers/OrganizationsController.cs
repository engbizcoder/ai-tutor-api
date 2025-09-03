namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Services.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class OrganizationsController(IOrgDeletionService orgDeletionService) : ControllerBase
{
    /// <summary>
    /// Disables an organization (sets to read-only mode).
    /// </summary>
    /// <param name="orgId">The organization ID to disable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{orgId:guid}/disable")]
    public async Task<IActionResult> DisableOrganization(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orgDeletionService.DisableOrgAsync(orgId, cancellationToken);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft-deletes an organization (marks for deletion after retention period).
    /// </summary>
    /// <param name="orgId">The organization ID to soft-delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{orgId:guid}")]
    public async Task<IActionResult> SoftDeleteOrganization(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orgDeletionService.SoftDeleteOrgAsync(orgId, cancellationToken);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Hard-deletes an organization immediately (permanent deletion).
    /// </summary>
    /// <param name="orgId">The organization ID to hard-delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{orgId:guid}/hard")]
    public async Task<IActionResult> HardDeleteOrganization(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await orgDeletionService.HardDeleteOrgAsync(orgId, cancellationToken);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
    }
}
