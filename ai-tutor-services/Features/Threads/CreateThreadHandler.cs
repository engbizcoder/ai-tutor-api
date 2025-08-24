namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;

public sealed class CreateThreadHandler(IThreadRepository threads) : IRequestHandler<CreateThreadRequest, ChatThread>
{
    public async Task<ChatThread> Handle(CreateThreadRequest request, CancellationToken ct = default)
    {
        var entity = new ChatThread
        {
            OrgId = request.OrgId,
            UserId = request.UserId,
            FolderId = request.FolderId,
            Title = request.Title,
            Status = request.Status,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
        };

        return await threads.AddAsync(entity, ct);
    }
}
