namespace Ai.Tutor.Services.Services;

using Ai.Tutor.Domain.Repositories;

/// <summary>
/// Coordinates file cleanup after thread/message/reference deletions to prevent orphaned blobs.
/// This orchestrator composes repository batch queries to efficiently discover candidate file IDs and delegates
/// actual file removal to <see cref="IFileCleanupService"/>. It does not depend on infrastructure concerns.
/// </summary>
public sealed class CleanupOrchestrator : ICleanupOrchestrator
{
    private readonly IMessageRepository messages;
    private readonly IAttachmentRepository attachments;
    private readonly IReferenceRepository references;
    private readonly IFileCleanupService fileCleanup;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanupOrchestrator"/> class.
    /// </summary>
    /// <param name="messages">Repository used to query message identifiers for given threads.</param>
    /// <param name="attachments">Repository used to query file identifiers attached to messages.</param>
    /// <param name="references">Repository used to query file identifiers referenced by threads.</param>
    /// <param name="fileCleanup">Service responsible for deleting orphaned files from storage and metadata.</param>
    public CleanupOrchestrator(
        IMessageRepository messages,
        IAttachmentRepository attachments,
        IReferenceRepository references,
        IFileCleanupService fileCleanup)
    {
        this.messages = messages;
        this.attachments = attachments;
        this.references = references;
        this.fileCleanup = fileCleanup;
    }

    /// <summary>
    /// Invoked after one or more threads have been deleted. Aggregates all file identifiers that were
    /// either referenced by references in those threads or attached to messages within those threads, and
    /// attempts to remove files that are now orphaned.
    /// </summary>
    /// <param name="threadIds">Deleted thread identifiers.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task OnThreadsDeletedAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return;
        }

        // Collect message IDs in the deleted threads
        var messageIds = await this.messages.ListIdsByThreadIdsAsync(threadIds, ct);

        // Collect file IDs referenced by references in those threads
        var refFileIds = await this.references.ListDistinctFileIdsByThreadIdsAsync(threadIds, ct);

        // Collect file IDs attached to messages in those threads
        var attFileIds = messageIds.Count == 0
            ? []
            : await this.attachments.ListDistinctFileIdsByMessageIdsAsync(messageIds, ct);

        // Union distinct file IDs
        var union = refFileIds
            .Concat(attFileIds)
            .Distinct()
            .ToArray();

        if (union.Length == 0)
        {
            return;
        }

        await this.fileCleanup.CleanupOrphanedFilesAsync(union, ct);
    }

    /// <summary>
    /// Invoked after references have been deleted. Determines which files might have become orphaned by
    /// scanning remaining references in the affected threads and delegates cleanup.
    /// </summary>
    /// <param name="threadIds">Thread identifiers whose references were deleted.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task OnReferencesDeletedAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return;
        }

        var fileIds = await this.references.ListDistinctFileIdsByThreadIdsAsync(threadIds, ct);
        if (fileIds.Count == 0)
        {
            return;
        }

        await this.fileCleanup.CleanupOrphanedFilesAsync(fileIds, ct);
    }

    /// <summary>
    /// Invoked after messages have been deleted. Determines which files might have become orphaned by
    /// scanning attachments for the deleted messages and delegates cleanup.
    /// </summary>
    /// <param name="messageIds">Deleted message identifiers.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task OnMessagesDeletedAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default)
    {
        if (messageIds.Count == 0)
        {
            return;
        }

        var fileIds = await this.attachments.ListDistinctFileIdsByMessageIdsAsync(messageIds, ct);
        if (fileIds.Count == 0)
        {
            return;
        }

        await this.fileCleanup.CleanupOrphanedFilesAsync(fileIds, ct);
    }
}
