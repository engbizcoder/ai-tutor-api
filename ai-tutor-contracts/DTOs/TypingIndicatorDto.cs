namespace Ai.Tutor.Contracts.DTOs;

using System;

public sealed class TypingIndicatorDto
{
    public Guid ThreadId { get; init; }

    public Guid? UserId { get; init; }

    public string? UserName { get; init; }

    public bool IsTyping { get; init; }

    public DateTime Timestamp { get; init; }
}
