namespace Ai.Tutor.Contracts.DTOs;

using System.ComponentModel.DataAnnotations;
using Ai.Tutor.Contracts.Enums;

public sealed class ThreadDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public Guid? FolderId { get; init; }

    public string? Title { get; init; }

    public ThreadStatus Status { get; init; }

    public decimal SortOrder { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}