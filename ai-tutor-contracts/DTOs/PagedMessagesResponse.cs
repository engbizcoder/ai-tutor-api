namespace Ai.Tutor.Contracts.DTOs;

public sealed class PagedMessagesResponse
{
    public IReadOnlyList<MessageDto> Items { get; init; } = [];

    public string? NextCursor { get; init; }

    public bool HasMore { get; init; }

    public int? TotalCount { get; init; }
}
