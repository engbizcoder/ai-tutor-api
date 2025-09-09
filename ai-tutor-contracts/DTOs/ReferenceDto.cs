namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class ReferenceDto
{
    public Guid Id { get; init; }

    public Guid ThreadId { get; init; }

    public Guid? MessageId { get; init; }

    public ReferenceType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Url { get; init; }

    public Guid? FileId { get; init; }

    public int? PageNumber { get; init; }

    public string? PreviewImgUrl { get; init; }

    public FileDto? File { get; init; }

    public DateTime CreatedAt { get; init; }
}
