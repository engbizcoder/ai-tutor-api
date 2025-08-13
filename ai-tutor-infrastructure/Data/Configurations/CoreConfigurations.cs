using Ai.Tutor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Tutor.Infrastructure.Data.Configurations;

public sealed class OrgConfiguration : IEntityTypeConfiguration<Org>
{
    public void Configure(EntityTypeBuilder<Org> b)
    {
        b.ToTable("orgs");
        b.HasKey(x => x.Id).HasName("pk_orgs");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_orgs_slug");
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>(); // mapped via enum type in migrations
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id).HasName("pk_users");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.PrimaryOrgId).HasColumnName("primary_org_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("ux_users_email");
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasOne<Org>()
            .WithMany()
            .HasForeignKey(x => x.PrimaryOrgId)
            .HasConstraintName("fk_users_primary_org")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrgMemberConfiguration : IEntityTypeConfiguration<OrgMember>
{
    public void Configure(EntityTypeBuilder<OrgMember> b)
    {
        b.ToTable("org_members");
        b.HasKey(x => new { x.OrgId, x.UserId }).HasName("pk_org_members");
        b.Property(x => x.OrgId).HasColumnName("org_id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.Role).HasColumnName("role").HasConversion<string>();
        b.Property(x => x.JoinedAt).HasColumnName("joined_at").IsRequired();
        b.HasOne<Org>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_org_members_orgs")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_org_members_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> b)
    {
        b.ToTable("folders");
        b.HasKey(x => x.Id).HasName("pk_folders");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.OrgId).HasColumnName("org_id");
        b.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        b.Property(x => x.ParentId).HasColumnName("parent_id");
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(x => x.Level).HasColumnName("level").IsRequired();
        b.Property(x => x.SortOrder).HasColumnName("sort_order").HasPrecision(12, 6).HasDefaultValue(1000m);
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(x => new { x.OrgId, x.OwnerUserId, x.ParentId }).HasDatabaseName("ix_folders_org_owner_parent");
        b.HasIndex(x => new { x.OwnerUserId, x.ParentId, x.Name }).IsUnique().HasDatabaseName("ux_folders_owner_parent_name");

        b.HasOne<Org>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_folders_orgs")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasConstraintName("fk_folders_users_owner")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Folder>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("fk_folders_parent")
            .OnDelete(DeleteBehavior.Restrict);

        // Check constraint for level/parent relationship will be added in migration via raw SQL
        // Partial index on status='active' will be added in migration via raw SQL
    }
}
