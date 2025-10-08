namespace Ai.Tutor.Contracts.DTOs;

using Ai.Tutor.Contracts.Enums;

/// <summary>
/// Represents a folder in the system with its properties and metadata.
/// </summary>
/// <param name="Id">The unique identifier of the folder.</param>
/// <param name="OwnerUserId">The ID of the user who owns this folder.</param>
/// <param name="ParentId">The ID of the parent folder, if this is a subfolder.</param>
/// <param name="Type">The type of the folder (e.g., Project or Folder).</param>
/// <param name="Status">The current status of the folder (Active, Archived, or Deleted).</param>
/// <param name="Name">The display name of the folder.</param>
/// <param name="Level">The depth level of the folder in the folder hierarchy.</param>
/// <param name="SortOrder">The sort order of the folder within its parent.</param>
/// <param name="CreatedAt">The date and time when the folder was created.</param>
/// <param name="UpdatedAt">The date and time when the folder was last updated.</param>
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
    DateTime UpdatedAt);

/// <summary>
/// Represents a request to create a new folder.
/// </summary>
public class CreateFolderRequest
{
    /// <summary>
    /// Gets or sets the type of the folder to create.
    /// </summary>
    public FolderType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the parent folder, if this is a subfolder.
    /// </summary>
    public Guid? ParentId { get; set; }
}

/// <summary>
/// Represents a request to update an existing folder's properties.
/// </summary>
public class UpdateFolderRequest
{
    /// <summary>
    /// Gets or sets the new name for the folder.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the new status for the folder.
    /// </summary>
    public FolderStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the new parent folder ID, if moving the folder.
    /// </summary>
    public Guid? NewParentId { get; set; }

    /// <summary>
    /// Gets or sets the new sort order for the folder within its parent.
    /// </summary>
    public decimal? SortOrder { get; set; }
}
