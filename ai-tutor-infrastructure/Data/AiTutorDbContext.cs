using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ai.Tutor.Infrastructure.Data;

public class AiTutorDbContext(DbContextOptions<AiTutorDbContext> options) : DbContext(options)
{
    public DbSet<Org> Orgs => this.Set<Org>();

    public DbSet<User> Users => this.Set<User>();

    public DbSet<OrgMember> OrgMembers => this.Set<OrgMember>();

    public DbSet<Folder> Folders => this.Set<Folder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register PostgreSQL enums matching approved types
        modelBuilder.HasPostgresEnum<OrgType>(name: "org_type_enum");
        modelBuilder.HasPostgresEnum<OrgRole>(name: "org_role_enum");
        modelBuilder.HasPostgresEnum<FolderType>(name: "folder_type_enum");
        modelBuilder.HasPostgresEnum<FolderStatus>(name: "folder_status_enum");

        // Apply configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiTutorDbContext).Assembly);
    }
}
