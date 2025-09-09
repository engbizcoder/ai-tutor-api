namespace Ai.Tutor.Infrastructure.Repositories;

using System.Text;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public sealed class ReferenceRepository(AiTutorDbContext db) : IReferenceRepository
{
    public async Task<Reference?> GetByIdAsync(Guid id, Guid orgId, CancellationToken ct = default)
    {
        // Fetch the reference and validate its thread belongs to the organization
        var rec = await db.DocumentReferences
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (rec is null)
        {
            return null;
        }

        var threadBelongs = await db.ChatThreads
            .AsNoTracking()
            .AnyAsync(t => t.Id == rec.ThreadId && t.OrgId == orgId, ct);

        if (!threadBelongs)
        {
            return null;
        }

        return ToDomain(rec);
    }

    public async Task<(IReadOnlyList<Reference> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        Guid orgId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        // Validate thread belongs to organization
        var threadExists = await db.ChatThreads
            .AsNoTracking()
            .AnyAsync(t => t.Id == threadId && t.OrgId == orgId, ct);

        if (!threadExists)
        {
            return (Array.Empty<Reference>(), null);
        }

        (DateTime? createdBefore, Guid? idBefore) = TryDecodeCursor(cursor);

        var query = db.DocumentReferences
            .AsNoTracking()
            .Where(x => x.ThreadId == threadId);

        if (createdBefore.HasValue && idBefore.HasValue)
        {
            // For DESC ordering: (created_at, id) < (createdBefore, idBefore)
            query = query.Where(
                x => x.CreatedAt < createdBefore.Value ||
                     (x.CreatedAt == createdBefore.Value && x.Id.CompareTo(idBefore.Value) < 0));
        }

        var records = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        if (records.Count == 0)
        {
            return (Array.Empty<Reference>(), null);
        }

        string? next = null;
        if (records.Count > pageSize)
        {
            var last = records[pageSize - 1];
            next = EncodeCursor(last.CreatedAt, last.Id);
            records = records.Take(pageSize).ToList();
        }

        var items = records.Select(ToDomain).ToList();
        return (items, next);
    }

    public async Task<Reference> AddAsync(Reference reference, Guid orgId, CancellationToken ct = default)
    {
        // Validate thread belongs to organization
        var threadExists = await db.ChatThreads
            .AsNoTracking()
            .AnyAsync(t => t.Id == reference.ThreadId && t.OrgId == orgId, ct);

        if (!threadExists)
        {
            throw new UnauthorizedAccessException($"Thread {reference.ThreadId} does not belong to organization {orgId}");
        }

        var rec = ToRecord(reference);
        await db.DocumentReferences.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    // Legacy method without tenancy validation - kept for backward compatibility
    public async Task<Reference> AddAsync(Reference reference, CancellationToken ct = default)
    {
        var rec = ToRecord(reference);
        await db.DocumentReferences.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    // Legacy method without tenancy validation - kept for backward compatibility  
    public async Task<(IReadOnlyList<Reference> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        (DateTime? createdBefore, Guid? idBefore) = TryDecodeCursor(cursor);

        var query = db.DocumentReferences
            .AsNoTracking()
            .Where(x => x.ThreadId == threadId);

        if (createdBefore.HasValue && idBefore.HasValue)
        {
            // For DESC ordering: (created_at, id) < (createdBefore, idBefore)
            query = query.Where(
                x => x.CreatedAt < createdBefore.Value ||
                     (x.CreatedAt == createdBefore.Value && x.Id.CompareTo(idBefore.Value) < 0));
        }

        var records = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        if (records.Count == 0)
        {
            return (Array.Empty<Reference>(), null);
        }

        string? next = null;
        if (records.Count > pageSize)
        {
            var last = records[pageSize - 1];
            next = EncodeCursor(last.CreatedAt, last.Id);
            records = records.Take(pageSize).ToList();
        }

        var items = records.Select(ToDomain).ToList();
        return (items, next);
    }

    public async Task<IReadOnlyList<Reference>> ListByMessageIdAsync(Guid messageId, CancellationToken ct = default)
    {
        var items = await db.DocumentReferences
            .AsNoTracking()
            .Where(r => r.MessageId == messageId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToListAsync(ct);
        return items.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Reference>> ListByFileIdAsync(Guid fileId, CancellationToken ct = default)
    {
        var items = await db.DocumentReferences
            .AsNoTracking()
            .Where(r => r.FileId == fileId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToListAsync(ct);
        return items.Select(ToDomain).ToList();
    }

    public async Task DeleteByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return;
        }

        await db.DocumentReferences
            .Where(r => threadIds.Contains(r.ThreadId))
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> ListDistinctFileIdsByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var ids = await db.DocumentReferences
            .AsNoTracking()
            .Where(r => threadIds.Contains(r.ThreadId) && r.FileId != null)
            .Select(r => r.FileId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return ids;
    }

    private static string EncodeCursor(DateTime createdAt, Guid id)
    {
        var payload = $"{createdAt.Ticks}:{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    private static (DateTime? CreatedAt, Guid? Id) TryDecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return (null, null);
        }

        try
        {
            var data = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = data.Split(':');
            if (parts.Length != 2)
            {
                return (null, null);
            }

            if (!long.TryParse(parts[0], out var ticks))
            {
                return (null, null);
            }

            if (!Guid.TryParse(parts[1], out var id))
            {
                return (null, null);
            }

            return (new DateTime(ticks, DateTimeKind.Utc), id);
        }
        catch
        {
            return (null, null);
        }
    }

    private static Reference ToDomain(ReferenceRecord x) => new()
    {
        Id = x.Id,
        ThreadId = x.ThreadId,
        MessageId = x.MessageId,
        Type = x.Type,
        Title = x.Title,
        Url = x.Url,
        FileId = x.FileId,
        PageNumber = x.PageNumber,
        PreviewImgUrl = x.PreviewImgUrl,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
    };

    private static ReferenceRecord ToRecord(Reference x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        ThreadId = x.ThreadId,
        MessageId = x.MessageId,
        Type = x.Type,
        Title = x.Title,
        Url = x.Url,
        FileId = x.FileId,
        PageNumber = x.PageNumber,
        PreviewImgUrl = x.PreviewImgUrl,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
    };
}
