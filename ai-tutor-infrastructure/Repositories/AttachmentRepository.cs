namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public sealed class AttachmentRepository(AiTutorDbContext db) : IAttachmentRepository
{
    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await db.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<Attachment> AddAsync(Attachment attachment, CancellationToken ct = default)
    {
        var rec = ToRecord(attachment);
        await db.Attachments.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task<IReadOnlyList<Attachment>> ListByMessageIdAsync(Guid messageId, CancellationToken ct = default)
    {
        var items = await db.Attachments
            .AsNoTracking()
            .Where(a => a.MessageId == messageId)
            .OrderBy(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
        return items.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Attachment>> ListByFileIdAsync(Guid fileId, CancellationToken ct = default)
    {
        var items = await db.Attachments
            .AsNoTracking()
            .Where(a => a.FileId == fileId)
            .OrderBy(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
        return items.Select(ToDomain).ToList();
    }

    public async Task DeleteByMessageIdsAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default)
    {
        if (messageIds.Count == 0)
        {
            return;
        }

        await db.Attachments
            .Where(a => messageIds.Contains(a.MessageId))
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> ListDistinctFileIdsByMessageIdsAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default)
    {
        if (messageIds.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var ids = await db.Attachments
            .AsNoTracking()
            .Where(a => messageIds.Contains(a.MessageId))
            .Select(a => a.FileId)
            .Distinct()
            .ToListAsync(ct);

        return ids;
    }

    private static Attachment ToDomain(AttachmentRecord x) => new()
    {
        Id = x.Id,
        MessageId = x.MessageId,
        FileId = x.FileId,
        Type = x.Type,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
    };

    private static AttachmentRecord ToRecord(Attachment x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        MessageId = x.MessageId,
        FileId = x.FileId,
        Type = x.Type,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
    };
}
