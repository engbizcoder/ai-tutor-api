using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ai.Tutor.Infrastructure.Repositories;

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
}
