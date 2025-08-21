namespace Ai.Tutor.Services.Features.Folders;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;

public sealed class DeleteFolderHandler(
    IThreadRepository threads,
    IMessageRepository messages,
    IFolderRepository folders,
    IUnitOfWork uow)
    : IRequestHandler<DeleteFolderRequest>
{
    public async Task Handle(DeleteFolderRequest request, CancellationToken ct = default)
    {
        // For now, only immediate folder level; nested hierarchy support can be added later.
        var threadIds = await threads.ListIdsByFolderAsync(request.OrgId, request.FolderId, ct);

        await uow.ExecuteInTransactionAsync(
            async innerCt =>
            {
                if (threadIds.Count > 0)
                {
                    await messages.DeleteByThreadIdsAsync(threadIds, innerCt);

                    // Delete threads (no bulk API yet)
                    foreach (var id in threadIds)
                    {
                        await threads.DeleteAsync(id, innerCt);
                    }
                }

                await folders.DeleteAsync(request.FolderId, innerCt);
            },
            ct);
    }
}
