namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

public sealed class FolderRecordConfiguration : IEntityTypeConfiguration<FolderRecord>
{
    public void Configure(EntityTypeBuilder<FolderRecord> b)
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

        b.HasOne<OrgRecord>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_folders_orgs")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasConstraintName("fk_folders_users_owner")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<FolderRecord>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("fk_folders_parent")
            .OnDelete(DeleteBehavior.Restrict);

        // Check constraint for level/parent relationship will be added in migration via raw SQL
        // Partial index on status='active' will be added in migration via raw SQL
    }
}
