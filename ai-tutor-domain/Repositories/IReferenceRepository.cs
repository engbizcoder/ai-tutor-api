namespace Ai.Tutor.Domain.Repositories;

using Entities;

/// <summary>
/// Repository abstraction for <see cref="Reference"/> entities that link messages/threads to external resources or files.
/// Provides tenancy-aware operations and list helpers to support pagination and cleanup orchestration.
/// </summary>
public interface IReferenceRepository
{
    /// <summary>
    /// Retrieves a reference by id while enforcing tenancy via the owning thread's organization.
    /// </summary>
    /// <param name="id">The reference identifier.</param>
    /// <param name="orgId">Organization id to validate thread ownership.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The reference if found and belongs to <paramref name="orgId"/>; otherwise null.</returns>
    Task<Reference?> GetByIdAsync(Guid id, Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new reference after validating that the related thread belongs to the specified organization.
    /// </summary>
    /// <param name="reference">The reference to add.</param>
    /// <param name="orgId">Organization id used to enforce tenancy via thread ownership.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created <see cref="Reference"/>.</returns>
    Task<Reference> AddAsync(Reference reference, Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Lists references within a thread (tenancy validated by <paramref name="orgId"/>) using cursor-based pagination.
    /// </summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <param name="orgId">Organization id used to validate thread ownership.</param>
    /// <param name="pageSize">Maximum number of items to return. Defaults applied if non-positive.</param>
    /// <param name="cursor">Opaque next-cursor returned by a previous call (descending by created_at,id).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple containing the items and a next cursor if more results are available.</returns>
    Task<(IReadOnlyList<Reference> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        Guid orgId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new reference without tenancy validation. Prefer the org-aware overload.
    /// </summary>
    /// <param name="reference">The reference to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created <see cref="Reference"/>.</returns>
    Task<Reference> AddAsync(Reference reference, CancellationToken ct = default);

    /// <summary>
    /// Lists references within a thread without tenancy validation. Prefer the org-aware overload.
    /// </summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <param name="pageSize">Maximum number of items to return.</param>
    /// <param name="cursor">Opaque next-cursor returned by a previous call.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple containing the items and next cursor.</returns>
    Task<(IReadOnlyList<Reference> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Lists references by message identifier.
    /// </summary>
    /// <param name="messageId">The message id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>References linked to the provided message.</returns>
    Task<IReadOnlyList<Reference>> ListByMessageIdAsync(Guid messageId, CancellationToken ct = default);

    /// <summary>
    /// Lists references that point to the specified file id.
    /// </summary>
    /// <param name="fileId">The file id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>References that reference the file.</returns>
    Task<IReadOnlyList<Reference>> ListByFileIdAsync(Guid fileId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all references that belong to any of the specified thread ids.
    /// </summary>
    /// <param name="threadIds">Thread identifiers for which to delete references.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);

    /// <summary>
    /// Returns distinct file IDs referenced by any references that belong to the specified thread IDs.
    /// Useful for cleanup orchestration after deleting threads or references.
    /// </summary>
    /// <param name="threadIds">Thread identifiers to scan.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Guid>> ListDistinctFileIdsByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);
}
