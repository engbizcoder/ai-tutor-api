namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IMessageRepository
{
    Task<(IReadOnlyList<ChatMessage> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct = default);

    Task DeleteByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);
}
