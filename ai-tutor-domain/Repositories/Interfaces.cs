using Ai.Tutor.Domain.Entities;

namespace Ai.Tutor.Domain.Repositories;

public interface IOrgRepository
{
    Task<Org?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Org> AddAsync(Org org, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);
}

public interface IOrgMemberRepository
{
    Task<bool> IsMemberAsync(Guid orgId, Guid userId, CancellationToken ct = default);

    Task AddAsync(OrgMember member, CancellationToken ct = default);
}

public interface IFolderRepository
{
    Task<Folder?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default);

    Task<List<Folder>> GetTreeAsync(Guid orgId, Guid ownerUserId, int maxDepth = 3, CancellationToken ct = default);

    Task<Folder> AddAsync(Folder folder, CancellationToken ct = default);

    Task UpdateAsync(Folder folder, CancellationToken ct = default);

    Task<bool> ExistsWithNameAsync(Guid ownerUserId, Guid? parentId, string name, CancellationToken ct = default);
}

public interface IThreadRepository
{
    Task<ChatThread?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default);

    Task<List<ChatThread>> ListByFolderAsync(Guid orgId, Guid? folderId, CancellationToken ct = default);

    Task<List<ChatThread>> ListByUserAsync(Guid orgId, Guid userId, CancellationToken ct = default);

    Task<ChatThread> AddAsync(ChatThread thread, CancellationToken ct = default);

    Task UpdateAsync(ChatThread thread, CancellationToken ct = default);
}

public interface IMessageRepository
{
    Task<(IReadOnlyList<ChatMessage> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct = default);
}
