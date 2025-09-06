namespace Ai.Tutor.Services.Features.Folders;

using Domain.Repositories;
using Mediation;
using Microsoft.Extensions.Logging;

public sealed class DeleteFolderHandler(
    IThreadRepository threads,
    IMessageRepository messages,
    IFolderRepository folders,
    IUnitOfWork uow,
    ILogger<DeleteFolderHandler> logger)
    : IRequestHandler<DeleteFolderRequest>
{
    public async Task Handle(DeleteFolderRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting folder {FolderId} in org {OrgId}", request.FolderId, request.OrgId);

        // Ensure folder exists and belongs to org; if not, signal 404 via ProblemDetails mapping
        var existing = await folders.GetAsync(request.FolderId, request.OrgId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Folder {request.FolderId} not found in org {request.OrgId}.");
        }

        // For now, only immediate folder level; nested hierarchy support can be added later.
        var threadIds = await threads.ListIdsByFolderAsync(request.OrgId, request.FolderId, ct);
        logger.LogDebug("Found {ThreadCount} threads in folder {FolderId}", threadIds.Count, request.FolderId);

        await uow.ExecuteInTransactionAsync(
            async innerCt =>
            {
                if (threadIds.Count > 0)
                {
                    await messages.DeleteByThreadIdsAsync(threadIds, innerCt);
                    logger.LogInformation(
                        "Deleted messages for {ThreadCount} threads in folder {FolderId}",
                        threadIds.Count,
                        request.FolderId);

                    // Delete threads (no bulk API yet)
                    foreach (var id in threadIds)
                    {
                        await threads.DeleteAsync(id, innerCt);
                        logger.LogDebug("Deleted thread {ThreadId}", id);
                    }
                }

                await folders.DeleteAsync(request.FolderId, innerCt);
                logger.LogInformation("Deleted folder {FolderId}", request.FolderId);
            },
            ct);
    }
}
