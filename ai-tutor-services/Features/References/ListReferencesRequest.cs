namespace Ai.Tutor.Services.Features.References;

using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class ListReferencesRequest : IRequest<(IReadOnlyList<Reference> Items, string? NextCursor)>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }

    public int PageSize { get; init; } = 20;

    public string? Cursor { get; init; }
}
