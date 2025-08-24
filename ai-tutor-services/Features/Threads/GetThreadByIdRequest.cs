namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Services.Mediation;

public sealed class GetThreadByIdRequest : IRequest<ChatThread>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }
}
