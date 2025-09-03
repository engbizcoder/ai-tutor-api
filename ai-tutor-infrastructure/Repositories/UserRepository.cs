namespace Ai.Tutor.Infrastructure.Repositories;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Domain.Repositories;
using Data;
using Data.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class UserRepository(AiTutorDbContext db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var rec = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);
        return rec is null ? null : ToDomain(rec);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        var rec = ToRecord(user);
        await db.Users.AddAsync(rec, ct);
        await db.SaveChangesAsync(ct);
        return ToDomain(rec);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync(ct);
    }

    public async Task<List<User>> GetUsersByPrimaryOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        var records = await db.Users
            .AsNoTracking()
            .Where(u => u.PrimaryOrgId == orgId)
            .ToListAsync(ct);

        return records.Select(ToDomain).ToList();
    }

    public async Task UpdatePrimaryOrgAsync(Guid userId, Guid newPrimaryOrgId, CancellationToken ct = default)
    {
        await db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                .SetProperty(u => u.PrimaryOrgId, newPrimaryOrgId)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public async Task<Org> EnsurePersonalOrgAsync(Guid userId, string userEmail, CancellationToken ct = default)
    {
        // Check if user already has a personal org
        var existingPersonalOrg = await db.Orgs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Type == OrgType.Personal &&
                     db.OrgMembers.Any(m => m.UserId == userId && m.OrgId == o.Id),
                ct);

        if (existingPersonalOrg != null)
        {
            return ToDomainOrg(existingPersonalOrg);
        }

        // Create new personal org
        var personalOrg = new OrgRecord
        {
            Id = Guid.NewGuid(),
            Name = $"{userEmail}'s Personal Org",
            Slug = $"personal-{userId:N}",
            Type = OrgType.Personal,
            LifecycleStatus = OrgLifecycleStatus.Active,
            RetentionDays = 90,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await db.Orgs.AddAsync(personalOrg, ct);

        // Add user as owner of the personal org
        var orgMember = new OrgMemberRecord
        {
            OrgId = personalOrg.Id,
            UserId = userId,
            Role = OrgRole.Owner,
            JoinedAt = DateTime.UtcNow,
        };

        await db.OrgMembers.AddAsync(orgMember, ct);
        await db.SaveChangesAsync(ct);

        return ToDomainOrg(personalOrg);
    }

    private static User ToDomain(UserRecord x) => new()
    {
        Id = x.Id,
        PrimaryOrgId = x.PrimaryOrgId,
        Name = x.Name,
        Email = x.Email,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static UserRecord ToRecord(User x) => new()
    {
        Id = x.Id == Guid.Empty ? Guid.NewGuid() : x.Id,
        PrimaryOrgId = x.PrimaryOrgId,
        Name = x.Name,
        Email = x.Email,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };

    private static Org ToDomainOrg(OrgRecord x) => new()
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
}
