namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public sealed class OrgMemberRepository(AiTutorDbContext db) : IOrgMemberRepository
{
    public Task<bool> IsMemberAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        return db.OrgMembers.AnyAsync(x => x.OrgId == orgId && x.UserId == userId, ct);
    }

    public async Task AddAsync(OrgMember member, CancellationToken ct = default)
    {
        var rec = ToRecord(member);
        await db.OrgMembers.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        await db.OrgMembers.Where(m => m.OrgId == orgId).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await db.OrgMembers.Where(m => m.UserId == userId).ExecuteDeleteAsync(ct);
    }

    private static OrgMemberRecord ToRecord(OrgMember x) => new()
    {
        OrgId = x.OrgId,
        UserId = x.UserId,
        Role = x.Role,
        JoinedAt = x.JoinedAt,
    };
}
