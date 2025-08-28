namespace Ai.Tutor.Services.Features.Threads;

using Mediation;

public sealed class DeleteThreadRequest : IRequest
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }
}
