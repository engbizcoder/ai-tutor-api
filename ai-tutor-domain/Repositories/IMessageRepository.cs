namespace Ai.Tutor.Domain.Repositories;

using Entities;

/// <summary>
/// Repository abstraction for chat messages within threads, supporting pagination and cleanup helpers.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Lists messages in a thread using forward cursor-based pagination (ordered by created_at, id ASC).
    /// </summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <param name="pageSize">Maximum number of items to return. Defaults applied if non-positive.</param>
    /// <param name="cursor">Opaque continuation token from previous call.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Page of messages and next cursor if more exist.</returns>
    Task<(IReadOnlyList<ChatMessage> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Persists a new <see cref="ChatMessage"/> record.
    /// </summary>
    /// <param name="message">The message entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created message with identifiers populated.</returns>
    Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a message by idempotency key ensuring the owning thread belongs to <paramref name="orgId"/>.
    /// </summary>
    /// <param name="idempotencyKey">Idempotency key supplied by client to deduplicate requests.</param>
    /// <param name="orgId">Organization scope to enforce tenancy via thread ownership.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The message if found and belongs to the organization; otherwise null.</returns>
    Task<ChatMessage?> GetByIdempotencyKeyAsync(string idempotencyKey, Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all messages belonging to any of the specified thread ids.
    /// </summary>
    /// <param name="threadIds">Threads for which to delete messages.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);

    /// <summary>
    /// Returns message IDs belonging to the specified thread IDs.
    /// </summary>
    /// <param name="threadIds">Thread identifiers to scan.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Guid>> ListIdsByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);
}
