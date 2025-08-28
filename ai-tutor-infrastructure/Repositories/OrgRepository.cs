namespace Ai.Tutor.Infrastructure.Repositories;

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

    public async Task<Org> AddAsync(Org org, CancellationToken ct = default)
    {
        var rec = ToRecord(org);
        await db.Orgs.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task DeleteAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.Orgs.Where(o => o.Id == orgId).ExecuteDeleteAsync(ct);
    }

    private static Org ToDomain(OrgRecord x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Slug = x.Slug,
        Type = x.Type,
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
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
