namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Domain.Repositories;
using Mediation;

public sealed class ListThreadsByUserHandler(IThreadRepository threads) : IRequestHandler<ListThreadsByUserRequest, List<ChatThread>>
{
    public async Task<List<ChatThread>> Handle(ListThreadsByUserRequest request, CancellationToken ct = default)
        => await threads.ListByUserAsync(request.OrgId, request.UserId, ct);
}
