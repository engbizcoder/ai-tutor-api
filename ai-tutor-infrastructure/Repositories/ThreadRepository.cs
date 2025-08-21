namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

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
        => this.ListByFolderAsync(orgId, folderId, 50, ct);

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
        => this.ListByUserAsync(orgId, userId, 50, ct);

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

    public async Task DeleteAsync(Guid threadId, CancellationToken ct = default)
    {
        await db.ChatThreads.Where(t => t.Id == threadId).ExecuteDeleteAsync(ct);
    }

    public async Task<List<Guid>> ListIdsByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        return await db.ChatThreads
            .AsNoTracking()
            .Where(t => t.OrgId == orgId)
            .Select(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<List<Guid>> ListIdsByFolderAsync(Guid orgId, Guid? folderId, CancellationToken ct = default)
    {
        var query = db.ChatThreads.AsNoTracking().Where(t => t.OrgId == orgId);
        if (folderId.HasValue)
        {
            query = query.Where(t => t.FolderId == folderId);
        }
        else
        {
            query = query.Where(t => t.FolderId == null);
        }

        return await query.Select(t => t.Id).ToListAsync(ct);
    }

    public async Task<List<Guid>> ListIdsByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.ChatThreads
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.ChatThreads
            .Where(t => t.OrgId == orgId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await db.ChatThreads
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync(ct);
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
