namespace Ai.Tutor.Services.Services;

/// <summary>
/// Service for cleaning up orphaned files when references are deleted.
/// </summary>
public interface IFileCleanupService
{
    /// <summary>
    /// Cleans up orphaned files that are no longer referenced by any entities.
    /// </summary>
    /// <param name="fileIds">Collection of file IDs to check for orphaned status.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of files cleaned up.</returns>
    Task<int> CleanupOrphanedFilesAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default);

    /// <summary>
    /// Performs a full cleanup scan for all orphaned files in the system.
    /// This is an expensive operation and should be run periodically as a background job.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of files cleaned up.</returns>
    Task<int> PerformFullCleanupAsync(CancellationToken ct = default);
}
