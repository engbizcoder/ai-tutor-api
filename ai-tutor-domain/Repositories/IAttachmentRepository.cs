namespace Ai.Tutor.Domain.Repositories;

using Entities;

/// <summary>
/// Repository abstraction for <see cref="Attachment"/> entities that associate messages with files.
/// Provides queries for listing by message or file and helpers to support cleanup orchestration.
/// </summary>
public interface IAttachmentRepository
{
    /// <summary>
    /// Retrieves an attachment by its identifier.
    /// </summary>
    /// <param name="id">The attachment identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The attachment if found; otherwise null.</returns>
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Persists a new <see cref="Attachment"/> record.
    /// </summary>
    /// <param name="attachment">The attachment entity to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created <see cref="Attachment"/>.</returns>
    Task<Attachment> AddAsync(Attachment attachment, CancellationToken ct = default);

    /// <summary>
    /// Lists attachments linked to the specified message.
    /// </summary>
    /// <param name="messageId">Message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of attachments ordered by creation time.</returns>
    Task<IReadOnlyList<Attachment>> ListByMessageIdAsync(Guid messageId, CancellationToken ct = default);

    /// <summary>
    /// Lists attachments that reference the given file identifier.
    /// </summary>
    /// <param name="fileId">The file identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of attachments that reference the file.</returns>
    Task<IReadOnlyList<Attachment>> ListByFileIdAsync(Guid fileId, CancellationToken ct = default);

    /// <summary>
    /// Deletes attachments that belong to the specified message identifiers.
    /// </summary>
    /// <param name="messageIds">Message identifiers to delete attachments for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByMessageIdsAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default);

    /// <summary>
    /// Returns distinct file IDs attached to the specified message IDs.
    /// </summary>
    /// <param name="messageIds">Messages to scan for associated file identifiers.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Guid>> ListDistinctFileIdsByMessageIdsAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default);
}
