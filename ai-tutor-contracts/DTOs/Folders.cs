using System.ComponentModel.DataAnnotations;
using Ai.Tutor.Contracts.Enums;

namespace Ai.Tutor.Contracts.DTOs;

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
    [Required]
    public FolderType Type { get; set; }

    [Required]
    [MinLength(1), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}

public class UpdateFolderRequest
{
    [MinLength(1), MaxLength(200)]
    public string? Name { get; set; }

    public FolderStatus? Status { get; set; }

    public Guid? NewParentId { get; set; }

    public decimal? SortOrder { get; set; }
}
