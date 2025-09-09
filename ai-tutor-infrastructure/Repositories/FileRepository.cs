namespace Ai.Tutor.Infrastructure.Repositories;

using System.Text;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public sealed class FileRepository(AiTutorDbContext db) : IFileRepository
{
    public async Task<StoredFile> AddAsync(StoredFile file, CancellationToken ct = default)
    {
        var rec = ToRecord(file);
        await db.Files.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, Guid orgId, CancellationToken ct = default)
    {
        var rec = await db.Files
            .AsNoTracking()
            .Where(f => f.Id == id && f.OrgId == orgId)
            .FirstOrDefaultAsync(ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await db.Files
            .AsNoTracking()
            .Where(f => f.Id == id)
            .FirstOrDefaultAsync(ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<(IReadOnlyList<StoredFile> Items, string? NextCursor)> ListByOwnerPagedAsync(
        Guid ownerId,
        Guid orgId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        (DateTime? createdAfter, Guid? idAfter) = TryDecodeCursor(cursor);

        var query = db.Files
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerId && x.OrgId == orgId);

        if (createdAfter.HasValue && idAfter.HasValue)
        {
            query = query.Where(
                x => x.CreatedAt > createdAfter.Value ||
                     (x.CreatedAt == createdAfter.Value && x.Id.CompareTo(idAfter.Value) > 0));
        }

        var records = await query
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        if (records.Count == 0)
        {
            return (Array.Empty<StoredFile>(), null);
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

    public async Task<StoredFile?> GetByChecksumAsync(string checksum, Guid orgId, CancellationToken ct = default)
    {
        var rec = await db.Files
            .AsNoTracking()
            .Where(f => f.OrgId == orgId && f.ChecksumSha256 == checksum)
            .FirstOrDefaultAsync(ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.Files
            .Where(f => f.OrgId == orgId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        await db.Files
            .Where(f => ids.Contains(f.Id))
            .ExecuteDeleteAsync(ct);
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

    private static StoredFile ToDomain(FileRecord x) => new()
    {
        Id = x.Id,
        OrgId = x.OrgId,
        OwnerUserId = x.OwnerUserId,
        FileName = x.FileName,
        ContentType = x.ContentType,
        StorageKey = x.StorageKey,
        StorageUrl = x.StorageUrl,
        SizeBytes = x.SizeBytes,
        ChecksumSha256 = x.ChecksumSha256,
        Pages = x.Pages,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static FileRecord ToRecord(StoredFile x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        OrgId = x.OrgId,
        OwnerUserId = x.OwnerUserId,
        FileName = x.FileName,
        ContentType = x.ContentType,
        StorageKey = x.StorageKey,
        StorageUrl = x.StorageUrl,
        SizeBytes = x.SizeBytes,
        ChecksumSha256 = x.ChecksumSha256,
        Pages = x.Pages,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
