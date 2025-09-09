namespace Ai.Tutor.Contracts.DTOs;

public sealed class FileDto
{
    public Guid Id { get; init; }

    public Guid OwnerUserId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public string? StorageUrl { get; init; }

    public long SizeBytes { get; init; }

    public int? Pages { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
