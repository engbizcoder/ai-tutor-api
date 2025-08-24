namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Microsoft.Extensions.Logging;

public sealed class DeleteThreadHandler(
    IMessageRepository messages,
    IThreadRepository threads,
    IUnitOfWork uow,
    ILogger<DeleteThreadHandler> logger)
    : IRequestHandler<DeleteThreadRequest>
{
    private static readonly Action<ILogger, Guid, Guid, Exception?> DeletingThread =
        LoggerMessage.Define<Guid, Guid>(
            LogLevel.Information,
            new EventId(1101, nameof(DeletingThread)),
            "Deleting thread {ThreadId} in org {OrgId}");

    private static readonly Action<ILogger, Guid, Exception?> DeletedThreadMessages =
        LoggerMessage.Define<Guid>(
            LogLevel.Debug,
            new EventId(1102, nameof(DeletedThreadMessages)),
            "Deleted messages for thread {ThreadId}");

    private static readonly Action<ILogger, Guid, Exception?> DeletedThread =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1103, nameof(DeletedThread)),
            "Deleted thread {ThreadId}");

    public async Task Handle(DeleteThreadRequest request, CancellationToken ct = default)
    {
        DeletingThread(logger, request.ThreadId, request.OrgId, null);
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
                DeletedThreadMessages(logger, request.ThreadId, null);
                await threads.DeleteAsync(request.ThreadId, innerCt);
                DeletedThread(logger, request.ThreadId, null);
            },
            ct);
    }
}
