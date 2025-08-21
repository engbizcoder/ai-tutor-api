namespace Ai.Tutor.Infrastructure.Data.Models;

using Ai.Tutor.Domain.Enums;

public sealed class FolderRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid OwnerUserId { get; set; }

    public Guid? ParentId { get; set; }

    public FolderType Type { get; set; }

    public FolderStatus Status { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }

    public decimal SortOrder { get; set; } = 1000m;

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
