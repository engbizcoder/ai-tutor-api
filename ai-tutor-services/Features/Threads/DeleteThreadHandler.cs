namespace Ai.Tutor.Services.Features.Threads;

using Domain.Repositories;
using Mediation;
using Microsoft.Extensions.Logging;

public sealed class DeleteThreadHandler(
    IMessageRepository messages,
    IThreadRepository threads,
    IUnitOfWork uow,
    ILogger<DeleteThreadHandler> logger)
    : IRequestHandler<DeleteThreadRequest>
{
    public async Task Handle(DeleteThreadRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting thread {ThreadId} in org {OrgId}", request.ThreadId, request.OrgId);
        var ids = new List<Guid> { request.ThreadId };

        // Ensure thread exists and belongs to org; if not, signal 404 via ProblemDetails mapping
        var existing = await threads.GetAsync(request.ThreadId, request.OrgId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}.");
        }

        await uow.ExecuteInTransactionAsync(
            async innerCt =>
            {
                await messages.DeleteByThreadIdsAsync(ids, innerCt);
                logger.LogDebug("Deleted messages for thread {ThreadId}", request.ThreadId);
                await threads.DeleteAsync(request.ThreadId, innerCt);
                logger.LogInformation("Deleted thread {ThreadId}", request.ThreadId);
            },
            ct);
    }
}
