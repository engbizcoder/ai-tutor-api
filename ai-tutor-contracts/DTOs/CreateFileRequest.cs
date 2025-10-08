namespace Ai.Tutor.Contracts.DTOs;

/// <summary>
/// Represents a request to create a new file in the system.
/// </summary>
public sealed class CreateFileRequest
{
    /// <summary>
    /// Gets the name of the file including its extension.
    /// </summary>
    /// <example>document.pdf.</example>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the MIME type of the file content.
    /// </summary>
    /// <example>application/pdf.</example>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the number of pages in the document, if applicable.
    /// </summary>
    /// <example>5.</example>
    public int? Pages { get; init; }
}
