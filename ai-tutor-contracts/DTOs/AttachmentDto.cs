namespace Ai.Tutor.Contracts.DTOs;

using Ai.Tutor.Contracts.Enums;

/// <summary>
/// Represents an attachment associated with a message in the system.
/// </summary>
public sealed class AttachmentDto
{
    /// <summary>
    /// Gets the unique identifier for the attachment.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the identifier of the message this attachment is associated with.
    /// </summary>
    public Guid MessageId { get; init; }

    /// <summary>
    /// Gets the identifier of the file associated with this attachment.
    /// </summary>
    public Guid FileId { get; init; }

    /// <summary>
    /// Gets the type of the attachment.
    /// </summary>
    public AttachmentType Type { get; init; }

    /// <summary>
    /// Gets the file details associated with this attachment, if loaded.
    /// </summary>
    public FileDto? File { get; init; }

    /// <summary>
    /// Gets the date and time when the attachment was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
