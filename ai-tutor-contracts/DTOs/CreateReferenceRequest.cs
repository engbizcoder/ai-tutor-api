namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class CreateReferenceRequest
{
    public Guid? MessageId { get; init; }

    public ReferenceType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Url { get; init; }

    public Guid? FileId { get; init; }

    public int? PageNumber { get; init; }

    public string? PreviewImgUrl { get; init; }
}
