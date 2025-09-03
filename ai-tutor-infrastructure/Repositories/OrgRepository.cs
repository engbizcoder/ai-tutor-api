namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Domain.Repositories;
using Data;
using Data.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class OrgRepository(AiTutorDbContext db) : IOrgRepository
{
    public async Task<Org?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var rec = await db.Orgs.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug, ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<Org?> GetByIdAsync(Guid orgId, CancellationToken ct = default)
    {
        var rec = await db.Orgs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId, ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<Org> AddAsync(Org org, CancellationToken ct = default)
    {
        var rec = ToRecord(org);
        await db.Orgs.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task<Org> UpdateAsync(Org org, CancellationToken ct = default)
    {
        var rec = ToRecord(org);
        db.Orgs.Update(rec);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task DeleteAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.Orgs.Where(o => o.Id == orgId).ExecuteDeleteAsync(ct);
    }

    public async Task UpdateLifecycleStatusAsync(Guid orgId, OrgLifecycleStatus status, DateTime? statusChangedAt = null, CancellationToken ct = default)
    {
        var updateTime = statusChangedAt ?? DateTime.UtcNow;

        await db.Orgs
            .Where(o => o.Id == orgId)
            .ExecuteUpdateAsync(
                setters => setters
                .SetProperty(o => o.LifecycleStatus, status)
                .SetProperty(o => o.UpdatedAt, updateTime)
                .SetProperty(o => o.DisabledAt, status == OrgLifecycleStatus.Disabled ? updateTime : (DateTime?)null)
                .SetProperty(o => o.DeletedAt, status == OrgLifecycleStatus.Deleted ? updateTime : (DateTime?)null),
                ct);
    }

    public async Task SoftDeleteAsync(Guid orgId, DateTime deletedAt, DateTime purgeScheduledAt, CancellationToken ct = default)
    {
        await db.Orgs
            .Where(o => o.Id == orgId)
            .ExecuteUpdateAsync(
                setters => setters
                .SetProperty(o => o.LifecycleStatus, OrgLifecycleStatus.Deleted)
                .SetProperty(o => o.DeletedAt, deletedAt)
                .SetProperty(o => o.PurgeScheduledAt, purgeScheduledAt)
                .SetProperty(o => o.UpdatedAt, deletedAt),
                ct);
    }

    public async Task<List<Org>> GetOrgsScheduledForPurgeAsync(CancellationToken ct = default)
    {
        var records = await db.Orgs
            .AsNoTracking()
            .Where(o => o.LifecycleStatus == OrgLifecycleStatus.Deleted && o.PurgeScheduledAt != null)
            .ToListAsync(ct);

        return records.Select(ToDomain).ToList();
    }

    public async Task<List<Org>> GetOrgsByLifecycleStatusAsync(OrgLifecycleStatus status, CancellationToken ct = default)
    {
        var records = await db.Orgs
            .AsNoTracking()
            .Where(o => o.LifecycleStatus == status)
            .ToListAsync(ct);

        return records.Select(ToDomain).ToList();
    }

    private static Org ToDomain(OrgRecord x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Slug = x.Slug,
        Type = x.Type,
        LifecycleStatus = x.LifecycleStatus,
        DisabledAt = x.DisabledAt,
        DeletedAt = x.DeletedAt,
        PurgeScheduledAt = x.PurgeScheduledAt,
        RetentionDays = x.RetentionDays,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static OrgRecord ToRecord(Org x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        Name = x.Name,
        Slug = x.Slug,
        Type = x.Type,
        LifecycleStatus = x.LifecycleStatus,
        DisabledAt = x.DisabledAt,
        DeletedAt = x.DeletedAt,
        PurgeScheduledAt = x.PurgeScheduledAt,
        RetentionDays = x.RetentionDays,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
