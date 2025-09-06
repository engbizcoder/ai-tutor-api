namespace Ai.Tutor.Contracts.DTOs;

using System.ComponentModel.DataAnnotations;

public sealed class ListMessagesRequest
{
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    public string? Cursor { get; init; }

    public bool IncludeMetadata { get; init; } = false;
}
