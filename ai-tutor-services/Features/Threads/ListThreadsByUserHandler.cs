namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;

public sealed class ListThreadsByUserHandler(IThreadRepository threads) : IRequestHandler<ListThreadsByUserRequest, List<ChatThread>>
{
    public async Task<List<ChatThread>> Handle(ListThreadsByUserRequest request, CancellationToken ct = default)
        => await threads.ListByUserAsync(request.OrgId, request.UserId, ct);
}
