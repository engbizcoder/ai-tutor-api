namespace Ai.Tutor.Contracts.DTOs;

using System.ComponentModel.DataAnnotations;
using Enums;

public sealed class CreateThreadRequest
{
    [Required]
    public Guid UserId { get; init; }

    public Guid? FolderId { get; init; }

    [MaxLength(200)]
    public string? Title { get; init; }

    public ThreadStatus Status { get; init; } = ThreadStatus.Active;

    public decimal? SortOrder { get; init; }
}
