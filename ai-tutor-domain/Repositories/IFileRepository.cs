namespace Ai.Tutor.Domain.Repositories;

using Entities;

/// <summary>
/// Repository abstraction for persisting and querying stored files metadata.
/// This interface is organization-aware and supports both tenant-scoped and internal maintenance operations.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Persists a new <see cref="StoredFile"/> record.
    /// </summary>
    /// <param name="file">The file metadata to store. The Id may be empty; an Id will be assigned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created <see cref="StoredFile"/> including generated identifiers.</returns>
    Task<StoredFile> AddAsync(StoredFile file, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a file by its identifier constrained to a specific organization.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <param name="orgId">The owning organization identifier to enforce tenancy.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The file if it exists and belongs to <paramref name="orgId"/>; otherwise null.</returns>
    Task<StoredFile?> GetByIdAsync(Guid id, Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a file by its identifier without enforcing organization scope.
    /// Use only for internal maintenance tasks such as cleanup orchestrations.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The file if it exists; otherwise null.</returns>
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists files owned by a specific user, scoped to an organization, using forward pagination.
    /// </summary>
    /// <param name="ownerId">The file owner (user) id.</param>
    /// <param name="orgId">The organization id.</param>
    /// <param name="pageSize">Maximum number of items to return.</param>
    /// <param name="cursor">Optional continuation token returned by a previous call.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A page of files and the next cursor if more data is available.</returns>
    Task<(IReadOnlyList<StoredFile> Items, string? NextCursor)> ListByOwnerPagedAsync(
        Guid ownerId,
        Guid orgId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a file by its checksum, scoped to an organization.
    /// </summary>
    /// <param name="checksum">The SHA-256 checksum string.</param>
    /// <param name="orgId">The organization id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The file if a match is found; otherwise null.</returns>
    Task<StoredFile?> GetByChecksumAsync(string checksum, Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all files belonging to the specified organization.
    /// </summary>
    /// <param name="orgId">The organization id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Deletes files by their IDs. Implementations should ignore IDs that do not exist.
    /// </summary>
    /// <param name="ids">Collection of file identifiers to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
}
