namespace Ai.Tutor.Api.IntegrationTests.Helpers;

using Infrastructure.Data;
using Infrastructure.Data.Models;

public static class DbSeed
{
    public static async Task<(OrgRecord Org, UserRecord User)> EnsureOrgAndUserAsync(AiTutorDbContext db)
    {
        var org = new OrgRecord
        {
            Id = Guid.NewGuid(),
            Name = "Test Org",
            Slug = "test-org",
            Type = Domain.Enums.OrgType.Business,
            LifecycleStatus = Domain.Enums.OrgLifecycleStatus.Active,
            RetentionDays = 90,
            CreatedAt = DateTime.UtcNow,
        };
        var user = new UserRecord
        {
            Id = Guid.NewGuid(),
            PrimaryOrgId = org.Id,
            Name = "Test User",
            Email = "user@example.com",
            CreatedAt = DateTime.UtcNow,
        };
        var member = new OrgMemberRecord
        {
            OrgId = org.Id,
            UserId = user.Id,
            Role = Domain.Enums.OrgRole.Member,
            JoinedAt = DateTime.UtcNow,
        };

        db.Orgs.Add(org);
        db.Users.Add(user);
        db.OrgMembers.Add(member);
        await db.SaveChangesAsync().ConfigureAwait(false);

        return (org, user);
    }

    public static async Task<OrgRecord> CreateOrgAsync(
        AiTutorDbContext db,
        string name,
        string slug)
    {
        var org = new OrgRecord
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Type = Domain.Enums.OrgType.Business,
            LifecycleStatus = Domain.Enums.OrgLifecycleStatus.Active,
            RetentionDays = 90,
            CreatedAt = DateTime.UtcNow,
        };

        db.Orgs.Add(org);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return org;
    }

    public static async Task<FolderRecord> EnsureFolderAsync(AiTutorDbContext db, Guid orgId, Guid ownerUserId)
    {
        var folder = new FolderRecord
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            OwnerUserId = ownerUserId,
            Type = Domain.Enums.FolderType.Folder,
            Status = Domain.Enums.FolderStatus.Active,
            Name = "Root",
            Level = 0,
            SortOrder = 1000m,
            CreatedAt = DateTime.UtcNow,
        };
        db.Folders.Add(folder);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return folder;
    }

    public static async Task<ThreadRecord> EnsureThreadAsync(AiTutorDbContext db, Guid orgId, Guid userId, Guid? folderId)
    {
        var thread = new ThreadRecord
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            UserId = userId,
            FolderId = folderId,
            Title = "Seed Thread",
            Status = Domain.Enums.ChatThreadStatus.Active,
            SortOrder = 1000m,
            CreatedAt = DateTime.UtcNow,
        };
        db.ChatThreads.Add(thread);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return thread;
    }

    public static async Task<MessageRecord> EnsureMessageAsync(AiTutorDbContext db, Guid threadId)
    {
        var msg = new MessageRecord
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            SenderType = Domain.Enums.SenderType.User,
            Status = Domain.Enums.MessageStatus.Sent,
            Content = "hello",
            CreatedAt = DateTime.UtcNow,
        };
        db.ChatMessages.Add(msg);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return msg;
    }
}
