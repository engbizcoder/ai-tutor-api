namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Services.Mediation;

public sealed class UpdateThreadCommand : IRequest
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }

    public string? Title { get; init; }

    public ChatThreadStatus? Status { get; init; }

    public Guid? NewFolderId { get; init; }

    public decimal? SortOrder { get; init; }
}
