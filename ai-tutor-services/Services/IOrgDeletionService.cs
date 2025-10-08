namespace Ai.Tutor.Services.Services;

public interface IOrgDeletionService
{
    /// <summary>
    /// Sets an organization to Disabled status (read-only mode).
    /// </summary>
    Task DisableOrgAsync(Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an organization and handles user membership cleanup.
    /// </summary>
    Task SoftDeleteOrgAsync(Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-deletes (purges) an organization and all its data permanently.
    /// </summary>
    Task HardDeleteOrgAsync(Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets organizations that are ready for hard deletion (past retention period).
    /// </summary>
    Task<List<Guid>> GetOrgsReadyForPurgeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Legacy method - now performs soft delete for safety.
    /// </summary>
    [Obsolete("Use SoftDeleteOrgAsync instead. This method now performs soft delete for safety.")]
    Task DeleteOrgAndDataAsync(Guid orgId, CancellationToken cancellationToken = default);
}
