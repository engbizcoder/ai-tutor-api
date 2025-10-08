namespace Ai.Tutor.Services.Features.References;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class CreateReferenceRequest : IRequest<Reference>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }

    public Guid? MessageId { get; init; }

    public ReferenceType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Url { get; init; }

    public Guid? FileId { get; init; }

    public int? PageNumber { get; init; }

    public string? PreviewImgUrl { get; init; }
}
