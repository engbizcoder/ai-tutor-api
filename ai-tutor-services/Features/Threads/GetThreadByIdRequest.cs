namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Mediation;

public sealed class GetThreadByIdRequest : IRequest<ChatThread>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }
}
