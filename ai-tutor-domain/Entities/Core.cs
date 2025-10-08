namespace Ai.Tutor.Domain.Entities;

using Ai.Tutor.Domain.Enums;

public interface IHasCreatedAt
{
    DateTime CreatedAt { get; set; }
}

public interface IHasUpdatedAt
{
    DateTime? UpdatedAt { get; set; }
}

public abstract class Entity
{
    public Guid Id { get; set; }
}

public abstract class AuditableEntity : Entity, IHasCreatedAt, IHasUpdatedAt
{
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public sealed class Org : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public OrgType Type { get; set; }

    public OrgLifecycleStatus LifecycleStatus { get; set; } = OrgLifecycleStatus.Active;

    public DateTime? DisabledAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? PurgeScheduledAt { get; set; }

    public int RetentionDays { get; set; } = 90;

    public string? MetadataJson { get; set; }

    public ICollection<OrgMember> Members { get; } = [];
}

public sealed class User : AuditableEntity
{
    public Guid PrimaryOrgId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }
}

public sealed class OrgMember
{
    public Guid OrgId { get; set; }

    public Guid UserId { get; set; }

    public OrgRole Role { get; set; }

    public DateTime JoinedAt { get; set; }
}

public sealed class Folder : AuditableEntity
{
    public Guid OrgId { get; set; }

    public Guid OwnerUserId { get; set; }

    public Guid? ParentId { get; set; }

    public FolderType Type { get; set; }

    public FolderStatus Status { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }

    public decimal SortOrder { get; set; } = 1000m;

    public string? MetadataJson { get; set; }
}

public sealed class ChatThread : AuditableEntity
{
    public Guid OrgId { get; set; }

    public Guid UserId { get; set; }

    public Guid? FolderId { get; set; }

    public string? Title { get; set; }

    public ChatThreadStatus Status { get; set; }

    public decimal SortOrder { get; set; } = 1000m;

    public string? MetadataJson { get; set; }
}

public sealed class ChatMessage : AuditableEntity
{
    public Guid ThreadId { get; set; }

    public SenderType SenderType { get; set; }

    public Guid? SenderId { get; set; }

    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    public string Content { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public string? IdempotencyKey { get; set; }
}

public sealed class StoredFile : AuditableEntity
{
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
}

public sealed class Attachment : Entity, IHasCreatedAt
{
    public Guid MessageId { get; set; }

    public Guid FileId { get; set; }

    public AttachmentType Type { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class Reference : Entity, IHasCreatedAt
{
    public Guid ThreadId { get; set; }

    public Guid? MessageId { get; set; }

    public ReferenceType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public Guid? FileId { get; set; }

    public int? PageNumber { get; set; }

    public string? PreviewImgUrl { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.Url) && !this.FileId.HasValue)
        {
            throw new ArgumentException("Either Url or FileId must be provided for a Reference.");
        }
    }
}
