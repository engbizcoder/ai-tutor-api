using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ai.Tutor.Infrastructure.Repositories;

public sealed class ThreadRepository(AiTutorDbContext db)
    : IThreadRepository
{
    public async Task<ChatThread?> GetAsync(Guid id, Guid orgId, CancellationToken ct = default)
    {
        var rec = await db.ChatThreads
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OrgId == orgId, ct);

        return rec is null ? null : ToDomain(rec);
    }

    // Overload required by IThreadRepository (no limit parameter)
    public Task<List<ChatThread>> ListByFolderAsync(Guid orgId, Guid? folderId, CancellationToken ct = default)
        => ListByFolderAsync(orgId, folderId, 50, ct);

    public async Task<List<ChatThread>> ListByFolderAsync(Guid orgId, Guid? folderId, int limit, CancellationToken ct = default)
    {
        if (limit <= 0)
        {
            limit = 50;
        }

        var query = db.ChatThreads.AsNoTracking().Where(x => x.OrgId == orgId);
        if (folderId.HasValue)
        {
            query = query.Where(x => x.FolderId == folderId);
        }
        else
        {
            query = query.Where(x => x.FolderId == null);
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);

        return items;
    }

    // Overload required by IThreadRepository (no limit parameter)
    public Task<List<ChatThread>> ListByUserAsync(Guid orgId, Guid userId, CancellationToken ct = default)
        => ListByUserAsync(orgId, userId, 50, ct);

    public async Task<List<ChatThread>> ListByUserAsync(Guid orgId, Guid userId, int limit, CancellationToken ct = default)
    {
        if (limit <= 0)
        {
            limit = 50;
        }

        var items = await db.ChatThreads
            .AsNoTracking()
            .Where(x => x.OrgId == orgId && x.UserId == userId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);

        return items;
    }

    public async Task<ChatThread> AddAsync(ChatThread thread, CancellationToken ct = default)
    {
        var rec = ToRecord(thread);
        await db.ChatThreads.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task UpdateAsync(ChatThread thread, CancellationToken ct = default)
    {
        var rec = ToRecord(thread);
        db.ChatThreads.Update(rec);
        await db.SaveChangesAsync(ct);
    }

    private static ChatThread ToDomain(ThreadRecord x) => new()
    {
        Id = x.Id,
        OrgId = x.OrgId,
        UserId = x.UserId,
        FolderId = x.FolderId,
        Title = x.Title,
        Status = x.Status,
        SortOrder = x.SortOrder,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static ThreadRecord ToRecord(ChatThread x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        OrgId = x.OrgId,
        UserId = x.UserId,
        FolderId = x.FolderId,
        Title = x.Title,
        Status = x.Status,
        SortOrder = x.SortOrder,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
