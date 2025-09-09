namespace Ai.Tutor.Domain.Repositories;

/// <summary>
/// Abstraction over file storage operations. Implementations may use local disk, cloud object storage, etc.
/// </summary>
public interface IFileStorageAdapter
{
    /// <summary>
    /// Uploads the given file stream to storage.
    /// </summary>
    /// <param name="fileStream">The file content stream. The implementation should not dispose the stream.</param>
    /// <param name="fileName">Original file name used to preserve extension where applicable.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The storage key that uniquely identifies the stored object.</returns>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Downloads the file content for the given storage key.
    /// </summary>
    /// <param name="storageKey">The storage key previously returned by <see cref="UploadAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A readable stream for the file content. The caller is responsible for disposing the stream.</returns>
    Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Generates a pre-signed URL for direct client download if supported by the implementation.
    /// </summary>
    /// <param name="storageKey">The storage key of the object.</param>
    /// <param name="expiry">How long the URL should remain valid.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A pre-signed URL string if supported; otherwise null.</returns>
    Task<string?> GetPresignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>
    /// Deletes the object identified by <paramref name="storageKey"/>.
    /// </summary>
    /// <param name="storageKey">The storage key of the object.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Checks whether an object exists for the given storage key.
    /// </summary>
    /// <param name="storageKey">The storage key of the object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the object exists; otherwise false.</returns>
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);
}
