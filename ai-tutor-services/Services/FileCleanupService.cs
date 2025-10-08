namespace Ai.Tutor.Services.Services;

using Ai.Tutor.Domain.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implements file cleanup operations by coordinating with domain repositories and the file storage adapter.
/// This service identifies orphaned files (no references and no attachments) and removes both the blob and its metadata.
/// </summary>
public sealed class FileCleanupService : IFileCleanupService
{
    private readonly IFileRepository files;
    private readonly IAttachmentRepository attachments;
    private readonly IReferenceRepository references;
    private readonly IFileStorageAdapter fileStorage;
    private readonly ILogger<FileCleanupService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCleanupService"/> class.
    /// </summary>
    /// <param name="files">Repository for file metadata queries and deletions.</param>
    /// <param name="attachments">Repository for attachment queries.</param>
    /// <param name="references">Repository for reference queries.</param>
    /// <param name="fileStorage">Adapter for deleting blobs from storage.</param>
    /// <param name="logger">Logger instance.</param>
    public FileCleanupService(
        IFileRepository files,
        IAttachmentRepository attachments,
        IReferenceRepository references,
        IFileStorageAdapter fileStorage,
        ILogger<FileCleanupService> logger)
    {
        this.files = files;
        this.attachments = attachments;
        this.references = references;
        this.fileStorage = fileStorage;
        this.logger = logger;
    }

    /// <summary>
    /// Cleans up a set of candidate files if they are determined to be orphaned (i.e., not referenced by any <c>Reference</c> or <c>Attachment</c>).
    /// For each orphaned file, the blob is deleted first from storage and then the metadata record is removed.
    /// </summary>
    /// <param name="fileIds">Collection of file identifiers to evaluate and possibly delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of files successfully cleaned up.</returns>
    public async Task<int> CleanupOrphanedFilesAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default)
    {
        if (fileIds.Count == 0)
        {
            return 0;
        }

        var orphanIds = new List<Guid>(fileIds.Count);

        foreach (var fileId in fileIds)
        {
            // Check if any references or attachments still point to this file
            var refs = await this.references.ListByFileIdAsync(fileId, ct);
            if (refs.Count > 0)
            {
                continue;
            }

            var atts = await this.attachments.ListByFileIdAsync(fileId, ct);
            if (atts.Count > 0)
            {
                continue;
            }

            orphanIds.Add(fileId);
        }

        if (orphanIds.Count == 0)
        {
            return 0;
        }

        var cleanedCount = 0;

        foreach (var fileId in orphanIds)
        {
            try
            {
                // Load file to get storage key
                var file = await this.files.GetByIdAsync(fileId, ct);
                if (file is null)
                {
                    continue;
                }

                // Delete from storage first
                await this.fileStorage.DeleteAsync(file.StorageKey, ct);

                // Then delete DB record
                await this.files.DeleteByIdsAsync([fileId], ct);

                cleanedCount++;
                this.logger.LogInformation("Cleaned up orphaned file {FileId} with storage key {StorageKey}", fileId, file.StorageKey);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to cleanup orphaned file {FileId}", fileId);
            }
        }

        return cleanedCount;
    }

    /// <summary>
    /// Placeholder for a full-system cleanup scan. Left unimplemented here to avoid leaking infrastructure concerns.
    /// Consider implementing a domain-level job that pages through files via <see cref="IFileRepository"/> and invokes this service.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Always returns 0 in the current implementation.</returns>
    public Task<int> PerformFullCleanupAsync(CancellationToken ct = default)
    {
        // TODO In absence of a dedicated query, a full scan is not implemented to avoid leaking infra concerns here.
        // Add a domain-level orchestrator to call FileCleanupService after thread/message/reference deletions
        // This method can be implemented via a domain service coordinating a paging scan using IFileRepository.
        this.logger.LogInformation("PerformFullCleanupAsync is not implemented to preserve clean architecture boundaries.");
        return Task.FromResult(0);
    }
}
