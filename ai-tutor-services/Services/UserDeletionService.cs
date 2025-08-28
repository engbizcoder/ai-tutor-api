namespace Ai.Tutor.Services.Services;

using Domain.Repositories;

public sealed class UserDeletionService(
    IUnitOfWork unitOfWork,
    IThreadRepository threadRepository,
    IMessageRepository messageRepository,
    IOrgMemberRepository orgMemberRepository,
    IUserRepository userRepository) : IUserDeletionService
{
    public async Task DeleteUserAndDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
        {
            // Collect thread ids owned by the user and delete messages
            var threadIds = await threadRepository.ListIdsByUserAsync(userId, ct);
            if (threadIds.Count > 0)
            {
                await messageRepository.DeleteByThreadIdsAsync(threadIds, ct);
            }

            // Delete user's threads
            await threadRepository.DeleteByUserAsync(userId, ct);

            // Delete org memberships for the user
            await orgMemberRepository.DeleteByUserAsync(userId, ct);

            // Finally, delete the user
            await userRepository.DeleteAsync(userId, ct);
        },
            cancellationToken);
    }
}
