namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Domain.Repositories;
using Mediation;

public sealed class GetThreadByIdHandler(IThreadRepository threads) : IRequestHandler<GetThreadByIdRequest, ChatThread>
{
    public async Task<ChatThread> Handle(GetThreadByIdRequest request, CancellationToken ct = default)
    {
        var item = await threads.GetAsync(request.ThreadId, request.OrgId, ct);
        if (item is null)
        {
            throw new KeyNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}.");
        }

        return item;
    }
}
