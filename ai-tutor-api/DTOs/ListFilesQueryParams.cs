namespace Ai.Tutor.Api.DTOs;

/// <summary>
/// Query parameters for listing files.
/// </summary>
public sealed class ListFilesQueryParams
{
    /// <summary>
    /// Gets or sets the number of files to return per page.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the cursor for pagination.
    /// </summary>
    public string? Cursor { get; set; }
}
