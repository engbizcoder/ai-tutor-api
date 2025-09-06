namespace Ai.Tutor.Contracts.DTOs;

using Ai.Tutor.Contracts.Enums;

public record FolderDto(
    Guid Id,
    Guid OwnerUserId,
    Guid? ParentId,
    FolderType Type,
    FolderStatus Status,
    string Name,
    int Level,
    decimal SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class CreateFolderRequest
{
    public FolderType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}

public class UpdateFolderRequest
{
    public string? Name { get; set; }

    public FolderStatus? Status { get; set; }

    public Guid? NewParentId { get; set; }

    public decimal? SortOrder { get; set; }
}
