namespace Ai.Tutor.Infrastructure.Data;

using Ai.Tutor.Infrastructure.Data.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Models;

public class AiTutorDbContext(DbContextOptions<AiTutorDbContext> options) : DbContext(options)
{
    public DbSet<OrgRecord> Orgs => this.Set<OrgRecord>();

    public DbSet<UserRecord> Users => this.Set<UserRecord>();

    public DbSet<OrgMemberRecord> OrgMembers => this.Set<OrgMemberRecord>();

    public DbSet<FolderRecord> Folders => this.Set<FolderRecord>();

    public DbSet<ThreadRecord> ChatThreads => this.Set<ThreadRecord>();

    public DbSet<MessageRecord> ChatMessages => this.Set<MessageRecord>();

    public DbSet<FileRecord> Files => this.Set<FileRecord>();

    public DbSet<AttachmentRecord> Attachments => this.Set<AttachmentRecord>();

    public DbSet<ReferenceRecord> DocumentReferences => this.Set<ReferenceRecord>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ApplyTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ApplyTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register PostgreSQL enums matching approved types
        modelBuilder.HasPostgresEnum<OrgType>(name: "org_type_enum");
        modelBuilder.HasPostgresEnum<OrgRole>(name: "org_role_enum");
        modelBuilder.HasPostgresEnum<OrgLifecycleStatus>(name: "org_lifecycle_status_enum");
        modelBuilder.HasPostgresEnum<FolderType>(name: "folder_type_enum");
        modelBuilder.HasPostgresEnum<FolderStatus>(name: "folder_status_enum");
        modelBuilder.HasPostgresEnum<ChatThreadStatus>(name: "thread_status_enum");
        modelBuilder.HasPostgresEnum<MessageStatus>(name: "message_status_enum");
        modelBuilder.HasPostgresEnum<SenderType>(name: "sender_type_enum");
        modelBuilder.HasPostgresEnum<AttachmentType>(name: "attachment_type_enum");
        modelBuilder.HasPostgresEnum<ReferenceType>(name: "reference_type_enum");

        // Apply configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiTutorDbContext).Assembly);
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in this.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is ICreatedAtEntity c && c.CreatedAt == default)
                {
                    c.CreatedAt = now;
                }

                if (entry.Entity is IUpdatedAtEntity u)
                {
                    u.UpdatedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IUpdatedAtEntity u)
                {
                    u.UpdatedAt = now;
                }

                // Prevent updates to CreatedAt on modified entities if present
                var createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProp is not null)
                {
                    createdAtProp.IsModified = false;
                }
            }
        }
    }
}
