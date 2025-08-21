using Ai.Tutor.Domain.Enums;

namespace Ai.Tutor.Domain.Entities;

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

    public string? MetadataJson { get; set; }

    public ICollection<OrgMember> Members { get; set; } = [];
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
}
