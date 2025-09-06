namespace Ai.Tutor.Services.Features.Messages;

using Domain.Entities;
using Mediation;

public sealed class ListMessagesRequest : IRequest<(IReadOnlyList<ChatMessage> Items, string? NextCursor)>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }

    public int PageSize { get; init; } = 20;

    public string? Cursor { get; init; }
}
