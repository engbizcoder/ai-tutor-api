namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Mediation;

public sealed class ListThreadsByUserRequest : IRequest<List<ChatThread>>
{
    public Guid OrgId { get; init; }

    public Guid UserId { get; init; }
}
