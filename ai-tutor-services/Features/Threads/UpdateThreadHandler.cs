namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;

public sealed class UpdateThreadHandler(IThreadRepository threads) : IRequestHandler<UpdateThreadCommand>
{
    public async Task Handle(UpdateThreadCommand request, CancellationToken ct = default)
    {
        var existing = await threads.GetAsync(request.ThreadId, request.OrgId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}.");
        }

        if (request.Title is not null)
        {
            existing.Title = request.Title;
        }

        if (request.Status.HasValue)
        {
            existing.Status = request.Status.Value;
        }

        if (request.NewFolderId.HasValue)
        {
            existing.FolderId = request.NewFolderId.Value;
        }

        if (request.SortOrder.HasValue)
        {
            existing.SortOrder = request.SortOrder.Value;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        await threads.UpdateAsync(existing, ct);
    }
}
