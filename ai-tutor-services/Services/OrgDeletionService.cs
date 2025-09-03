namespace Ai.Tutor.Services.Services;

using Domain.Repositories;
using Domain.Enums;

public sealed class OrgDeletionService(
    IUnitOfWork unitOfWork,
    IThreadRepository threadRepository,
    IMessageRepository messageRepository,
    IFolderRepository folderRepository,
    IOrgMemberRepository orgMemberRepository,
    IOrgRepository orgRepository,
    IUserRepository userRepository) : IOrgDeletionService
{
    /// <summary>
    /// Soft-deletes an organization by setting it to Disabled status (read-only)
    /// </summary>
    public async Task DisableOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
        {
            var org = await orgRepository.GetByIdAsync(orgId, ct);
            if (org is null)
            {
                throw new InvalidOperationException($"Organization {orgId} not found");
            }

            if (org.LifecycleStatus != OrgLifecycleStatus.Active)
            {
                throw new InvalidOperationException($"Organization {orgId} is not in Active status");
            }

            await orgRepository.UpdateLifecycleStatusAsync(orgId, OrgLifecycleStatus.Disabled, DateTime.UtcNow, ct);
        },
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes an organization by setting it to Deleted status and handling user memberships
    /// </summary>
    public async Task SoftDeleteOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
        {
            var org = await orgRepository.GetByIdAsync(orgId, ct);
            if (org is null)
            {
                throw new InvalidOperationException($"Organization {orgId} not found");
            }

            if (org.LifecycleStatus is OrgLifecycleStatus.Deleted or OrgLifecycleStatus.Purged)
            {
                throw new InvalidOperationException($"Organization {orgId} is already deleted");
            }

            var deletedAt = DateTime.UtcNow;
            var purgeScheduledAt = deletedAt.AddDays(org.RetentionDays);

            // Soft delete the organization with proper status and purge scheduling
            await orgRepository.SoftDeleteAsync(orgId, deletedAt, purgeScheduledAt, ct);

            // Handle user memberships - remove all members but preserve user accounts
            await this.HandleUserMembershipsOnOrgDeletionAsync(orgId, ct);
        },
            cancellationToken);
    }

    /// <summary>
    /// Hard-deletes (purges) an organization and all its data permanently
    /// </summary>
    public async Task HardDeleteOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
        {
            var org = await orgRepository.GetByIdAsync(orgId, ct);
            if (org == null)
            {
                throw new InvalidOperationException($"Organization {orgId} not found");
            }

            if (org.LifecycleStatus != OrgLifecycleStatus.Deleted)
            {
                throw new InvalidOperationException($"Organization {orgId} must be in Deleted status before hard delete");
            }

            if (org.PurgeScheduledAt > DateTime.UtcNow)
            {
                throw new InvalidOperationException($"Organization {orgId} retention period has not expired yet");
            }

            // Purge all org-scoped data
            await this.PurgeOrgDataAsync(orgId, ct);

            // Completely delete the organization record from the database
            await orgRepository.DeleteAsync(orgId, ct);
        },
            cancellationToken);
    }

    /// <summary>
    /// Legacy method - now performs soft delete instead of hard delete
    /// </summary>
    [Obsolete("Use SoftDeleteOrgAsync instead. This method now performs soft delete for safety.")]
    public async Task DeleteOrgAndDataAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await this.SoftDeleteOrgAsync(orgId, cancellationToken);
    }

    /// <summary>
    /// Gets organizations that are ready for hard deletion (past retention period)
    /// </summary>
    public async Task<List<Guid>> GetOrgsReadyForPurgeAsync(CancellationToken cancellationToken = default)
    {
        var orgs = await orgRepository.GetOrgsScheduledForPurgeAsync(cancellationToken);
        return orgs.Where(o => o.PurgeScheduledAt <= DateTime.UtcNow)
                  .Select(o => o.Id)
                  .ToList();
    }

    private async Task HandleUserMembershipsOnOrgDeletionAsync(Guid orgId, CancellationToken ct)
    {
        // Get users who have this org as their primary org
        var affectedUsers = await userRepository.GetUsersByPrimaryOrgAsync(orgId, ct);

        foreach (var user in affectedUsers)
        {
            // Create or find their personal org
            var personalOrg = await userRepository.EnsurePersonalOrgAsync(user.Id, user.Email, ct);

            // Update their primary org to the personal org
            await userRepository.UpdatePrimaryOrgAsync(user.Id, personalOrg.Id, ct);
        }

        // Remove all memberships from the deleted org
        await orgMemberRepository.DeleteByOrgAsync(orgId, ct);
    }

    private async Task PurgeOrgDataAsync(Guid orgId, CancellationToken ct)
    {
        // 1) Collect thread IDs and delete messages
        var threadIds = await threadRepository.ListIdsByOrgAsync(orgId, ct);
        if (threadIds.Count > 0)
        {
            await messageRepository.DeleteByThreadIdsAsync(threadIds, ct);
        }

        // 2) Delete threads
        await threadRepository.DeleteByOrgAsync(orgId, ct);

        // 3) Delete folders
        await folderRepository.DeleteByOrgAsync(orgId, ct);

        // 4) Delete org members (should already be done in soft delete, but ensure cleanup)
        await orgMemberRepository.DeleteByOrgAsync(orgId, ct);

        // TODO: Future enhancements
        // - Delete attachments and files from object storage
        // - Notify RAG service to drop indexes/corpora
        // - Delete usage logs and message events
        // - Clean up any other org-scoped data
    }
}
