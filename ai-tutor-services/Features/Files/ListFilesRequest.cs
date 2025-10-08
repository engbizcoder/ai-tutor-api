namespace Ai.Tutor.Services.Features.Files;

using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class ListFilesRequest : IRequest<(IReadOnlyList<StoredFile> Items, string? NextCursor)>
{
    public Guid OrgId { get; init; }

    public Guid OwnerUserId { get; init; }

    public int PageSize { get; init; } = 20;

    public string? Cursor { get; init; }
}
