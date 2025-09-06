namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class CreateThreadRequest
{
    public Guid UserId { get; init; }

    public Guid? FolderId { get; init; }

    public string? Title { get; init; }

    public ThreadStatus Status { get; init; } = ThreadStatus.Active;

    public decimal? SortOrder { get; init; }
}
