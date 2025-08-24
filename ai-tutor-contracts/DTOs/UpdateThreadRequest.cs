namespace Ai.Tutor.Contracts.DTOs;

using System.ComponentModel.DataAnnotations;
using Ai.Tutor.Contracts.Enums;

public sealed class UpdateThreadRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    public ThreadStatus? Status { get; init; }

    public Guid? NewFolderId { get; init; }

    public decimal? SortOrder { get; init; }
}
