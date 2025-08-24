namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Services.Mediation;

public sealed class ListThreadsByUserRequest : IRequest<List<ChatThread>>
{
    public Guid OrgId { get; init; }

    public Guid UserId { get; init; }
}
