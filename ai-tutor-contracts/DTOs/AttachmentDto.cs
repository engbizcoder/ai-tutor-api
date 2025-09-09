namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class AttachmentDto
{
    public Guid Id { get; init; }

    public Guid MessageId { get; init; }

    public Guid FileId { get; init; }

    public AttachmentType Type { get; init; }

    public FileDto? File { get; init; }

    public DateTime CreatedAt { get; init; }
}
