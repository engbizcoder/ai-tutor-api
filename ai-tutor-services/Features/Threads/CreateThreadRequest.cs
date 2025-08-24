namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Services.Mediation;

public sealed class CreateThreadRequest : IRequest<ChatThread>
{
    public Guid OrgId { get; init; }

    public Guid UserId { get; init; }

    public Guid? FolderId { get; init; }

    public string? Title { get; init; }

    public ChatThreadStatus Status { get; init; }

    public decimal SortOrder { get; init; } = 1000m;
}
