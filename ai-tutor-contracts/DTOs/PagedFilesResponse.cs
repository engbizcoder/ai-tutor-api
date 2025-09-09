namespace Ai.Tutor.Contracts.DTOs;

public sealed class PagedFilesResponse
{
    public IReadOnlyList<FileDto> Items { get; init; } = [];

    public string? NextCursor { get; init; }

    public bool HasMore { get; init; }

    public int? TotalCount { get; init; }
}
