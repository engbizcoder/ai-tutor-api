namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class MessageDto
{
    public Guid Id { get; init; }

    public Guid ThreadId { get; init; }

    public SenderType SenderType { get; init; }

    public Guid? SenderId { get; init; }

    public MessageStatus Status { get; init; }

    public string Content { get; init; } = string.Empty;

    public string? MetadataJson { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
