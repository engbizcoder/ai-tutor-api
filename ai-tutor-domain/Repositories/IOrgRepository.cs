namespace Ai.Tutor.Domain.Repositories;

using Entities;
using Enums;

public interface IOrgRepository
{
    Task<Org?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Org?> GetByIdAsync(Guid orgId, CancellationToken ct = default);

    Task<Org> AddAsync(Org org, CancellationToken ct = default);

    Task<Org> UpdateAsync(Org org, CancellationToken ct = default);

    Task DeleteAsync(Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Updates the lifecycle status of an organization.
    /// </summary>
    Task UpdateLifecycleStatusAsync(Guid orgId, OrgLifecycleStatus status, DateTime? statusChangedAt = null, CancellationToken ct = default);

    /// <summary>
    /// Soft deletes an organization by setting status to Deleted and scheduling purge.
    /// </summary>
    Task SoftDeleteAsync(Guid orgId, DateTime deletedAt, DateTime purgeScheduledAt, CancellationToken ct = default);

    /// <summary>
    /// Gets organizations that are scheduled for purge (deleted + past retention period).
    /// </summary>
    Task<List<Org>> GetOrgsScheduledForPurgeAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets organizations by lifecycle status.
    /// </summary>
    Task<List<Org>> GetOrgsByLifecycleStatusAsync(OrgLifecycleStatus status, CancellationToken ct = default);
}
