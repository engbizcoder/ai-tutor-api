namespace Ai.Tutor.Infrastructure.Data.Models;

using Interfaces;

public sealed class FileRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid OwnerUserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string? StorageUrl { get; set; }

    public long SizeBytes { get; set; }

    public string? ChecksumSha256 { get; set; }

    public int? Pages { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public OrgRecord? Org { get; set; }

    public UserRecord? OwnerUser { get; set; }
}
