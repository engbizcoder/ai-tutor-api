namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IThreadRepository
{
    Task<ChatThread?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default);

    Task<List<ChatThread>> ListByFolderAsync(Guid orgId, Guid? folderId, CancellationToken ct = default);

    Task<List<ChatThread>> ListByUserAsync(Guid orgId, Guid userId, CancellationToken ct = default);

    Task<ChatThread> AddAsync(ChatThread thread, CancellationToken ct = default);

    Task UpdateAsync(ChatThread thread, CancellationToken ct = default);

    Task DeleteAsync(Guid threadId, CancellationToken ct = default);

    Task<List<Guid>> ListIdsByFolderAsync(Guid orgId, Guid? folderId, CancellationToken ct = default);

    Task<List<Guid>> ListIdsByOrgAsync(Guid orgId, CancellationToken ct = default);

    Task<List<Guid>> ListIdsByUserAsync(Guid userId, CancellationToken ct = default);

    Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default);

    Task DeleteByUserAsync(Guid userId, CancellationToken ct = default);
}
