using System.Text;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ai.Tutor.Infrastructure.Repositories;

public sealed class MessageRepository(AiTutorDbContext db)
    : IMessageRepository
{
    public async Task<(IReadOnlyList<ChatMessage> Items, string? NextCursor)> ListByThreadPagedAsync(
        Guid threadId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        (DateTime? createdAfter, Guid? idAfter) = TryDecodeCursor(cursor);

        var query = db.ChatMessages
            .AsNoTracking()
            .Where(x => x.ThreadId == threadId);

        if (createdAfter.HasValue && idAfter.HasValue)
        {
            // (created_at, id) > (createdAfter, idAfter) in ASC ordering
            query = query.Where(
                messageRecord =>
                messageRecord.CreatedAt > createdAfter.Value ||
                (messageRecord.CreatedAt == createdAfter.Value && messageRecord.Id.CompareTo(idAfter.Value) > 0));
        }

        var records = await query
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        if (records.Count == 0)
        {
            return (Array.Empty<ChatMessage>(), null);
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

    public async Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        var rec = ToRecord(message);
        await db.ChatMessages.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task<ChatMessage?> GetByIdempotencyKeyAsync(string idempotencyKey, Guid orgId, CancellationToken ct = default)
    {
        var record = await db.ChatMessages
            .AsNoTracking()
            .Include(m => m.Thread)
            .Where(m => m.IdempotencyKey == idempotencyKey && m.Thread!.OrgId == orgId)
            .FirstOrDefaultAsync(ct);

        return record == null ? null : ToDomain(record);
    }

    public async Task DeleteByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return;
        }

        await db.ChatMessages
            .Where(m => threadIds.Contains(m.ThreadId))
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> ListIdsByThreadIdsAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default)
    {
        if (threadIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var ids = await db.ChatMessages
            .AsNoTracking()
            .Where(m => threadIds.Contains(m.ThreadId))
            .Select(m => m.Id)
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

    private static ChatMessage ToDomain(MessageRecord x) => new()
    {
        Id = x.Id,
        ThreadId = x.ThreadId,
        SenderType = x.SenderType,
        SenderId = x.SenderId,
        Status = x.Status,
        Content = x.Content,
        MetadataJson = x.MetadataJson,
        IdempotencyKey = x.IdempotencyKey,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static MessageRecord ToRecord(ChatMessage x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        ThreadId = x.ThreadId,
        SenderType = x.SenderType,
        SenderId = x.SenderId,
        Status = x.Status,
        Content = x.Content,
        MetadataJson = x.MetadataJson,
        IdempotencyKey = x.IdempotencyKey,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
