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
    private static readonly Action<ILogger, Guid, Guid, Exception?> DeletingFolder =
        LoggerMessage.Define<Guid, Guid>(
            LogLevel.Information,
            new EventId(1001, nameof(DeletingFolder)),
            "Deleting folder {FolderId} in org {OrgId}");

    private static readonly Action<ILogger, int, Guid, Exception?> FoundThreads =
        LoggerMessage.Define<int, Guid>(
            LogLevel.Debug,
            new EventId(1002, nameof(FoundThreads)),
            "Found {ThreadCount} threads in folder {FolderId}");

    private static readonly Action<ILogger, int, Guid, Exception?> DeletedMessages =
        LoggerMessage.Define<int, Guid>(
            LogLevel.Information,
            new EventId(1003, nameof(DeletedMessages)),
            "Deleted messages for {ThreadCount} threads in folder {FolderId}");

    private static readonly Action<ILogger, Guid, Exception?> DeletedThread =
        LoggerMessage.Define<Guid>(
            LogLevel.Debug,
            new EventId(1004, nameof(DeletedThread)),
            "Deleted thread {ThreadId}");

    private static readonly Action<ILogger, Guid, Exception?> DeletedFolder =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1005, nameof(DeletedFolder)),
            "Deleted folder {FolderId}");

    public async Task Handle(DeleteFolderRequest request, CancellationToken ct = default)
    {
        DeletingFolder(logger, request.FolderId, request.OrgId, null);

        // Ensure folder exists and belongs to org; if not, signal 404 via ProblemDetails mapping
        var existing = await folders.GetAsync(request.FolderId, request.OrgId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Folder {request.FolderId} not found in org {request.OrgId}.");
        }

        // For now, only immediate folder level; nested hierarchy support can be added later.
        var threadIds = await threads.ListIdsByFolderAsync(request.OrgId, request.FolderId, ct);
        FoundThreads(logger, threadIds.Count, request.FolderId, null);

        await uow.ExecuteInTransactionAsync(
            async innerCt =>
            {
                if (threadIds.Count > 0)
                {
                    await messages.DeleteByThreadIdsAsync(threadIds, innerCt);
                    DeletedMessages(logger, threadIds.Count, request.FolderId, null);

                    // Delete threads (no bulk API yet)
                    foreach (var id in threadIds)
                    {
                        await threads.DeleteAsync(id, innerCt);
                        DeletedThread(logger, id, null);
                    }
                }

                await folders.DeleteAsync(request.FolderId, innerCt);
                DeletedFolder(logger, request.FolderId, null);
            },
            ct);
    }
}
