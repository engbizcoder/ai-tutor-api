namespace Ai.Tutor.Services.Services;

using Domain.Repositories;

public sealed class OrgDeletionService(
    IUnitOfWork unitOfWork,
    IThreadRepository threadRepository,
    IMessageRepository messageRepository,
    IFolderRepository folderRepository,
    IOrgMemberRepository orgMemberRepository,
    IOrgRepository orgRepository) : IOrgDeletionService
{
    public async Task DeleteOrgAndDataAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
        {
            // 1) Collect thread IDs and delete messages
            var threadIds = await threadRepository.ListIdsByOrgAsync(orgId, ct);
            if (threadIds.Count > 0)
            {
                await messageRepository.DeleteByThreadIdsAsync(threadIds, ct);
            }

            // 2) Delete threads
            await threadRepository.DeleteByOrgAsync(orgId, ct);

            // 3) Delete folders
            await folderRepository.DeleteByOrgAsync(orgId, ct);

            // 4) Delete org members
            await orgMemberRepository.DeleteByOrgAsync(orgId, ct);

            // 5) Delete org
            await orgRepository.DeleteAsync(orgId, ct);
        },
            cancellationToken);
    }
}
