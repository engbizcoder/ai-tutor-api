namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IFolderRepository
{
    Task<Folder?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default);

    Task<List<Folder>> GetTreeAsync(Guid orgId, Guid ownerUserId, int maxDepth = 3, CancellationToken ct = default);

    Task<Folder> AddAsync(Folder folder, CancellationToken ct = default);

    Task UpdateAsync(Folder folder, CancellationToken ct = default);

    Task<bool> ExistsWithNameAsync(Guid ownerUserId, Guid? parentId, string name, CancellationToken ct = default);

    Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default);

    Task DeleteAsync(Guid folderId, CancellationToken ct = default);
}
