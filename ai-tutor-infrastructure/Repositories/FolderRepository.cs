namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Repositories;
using Data;
using Data.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class FolderRepository(AiTutorDbContext db)
    : IFolderRepository
{
    public async Task<Folder?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default)
    {
        var rec = await db.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OrgId == orgId, ct);

        return rec is null ? null : ToDomain(rec);
    }

    public async Task<List<Folder>> GetTreeAsync(Guid orgId, Guid ownerUserId, int maxDepth = 3, CancellationToken ct = default)
    {
        if (maxDepth < 1)
        {
            maxDepth = 1;
        }

        var items = await db.Folders
            .AsNoTracking()
            .Where(x => x.OrgId == orgId && x.OwnerUserId == ownerUserId && x.Level <= maxDepth)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.ParentId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

        return items.Select(ToDomain).ToList();
    }

    public async Task<Folder> AddAsync(Folder folder, CancellationToken ct = default)
    {
        var rec = ToRecord(folder);
        await db.Folders.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task UpdateAsync(Folder folder, CancellationToken ct = default)
    {
        var rec = ToRecord(folder);
        db.Folders.Update(rec);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsWithNameAsync(Guid ownerUserId, Guid? parentId, string name, CancellationToken ct = default)
    {
        return db.Folders.AnyAsync(x => x.OwnerUserId == ownerUserId && x.ParentId == parentId && x.Name == name, ct);
    }

    public async Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.Folders.Where(f => f.OrgId == orgId).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAsync(Guid folderId, CancellationToken ct = default)
    {
        await db.Folders.Where(f => f.Id == folderId).ExecuteDeleteAsync(ct);
    }

    private static Folder ToDomain(FolderRecord x) => new()
    {
        Id = x.Id,
        OrgId = x.OrgId,
        OwnerUserId = x.OwnerUserId,
        ParentId = x.ParentId,
        Type = x.Type,
        Status = x.Status,
        Name = x.Name,
        Level = x.Level,
        SortOrder = x.SortOrder,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static FolderRecord ToRecord(Folder x) =>
        new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        OrgId = x.OrgId,
        OwnerUserId = x.OwnerUserId,
        ParentId = x.ParentId,
        Type = x.Type,
        Status = x.Status,
        Name = x.Name,
        Level = x.Level,
        SortOrder = x.SortOrder,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
