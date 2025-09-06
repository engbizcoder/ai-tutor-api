namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class CreateMessageRequest
{
    public string Content { get; init; } = string.Empty;

    public SenderType SenderType { get; init; } = SenderType.User;

    public Guid? SenderId { get; init; }

    public string? MetadataJson { get; init; }

    public string? IdempotencyKey { get; init; }
}
