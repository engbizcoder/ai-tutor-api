namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

public sealed class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> b)
    {
        b.ToTable("files");
        b.HasKey(x => x.Id).HasName("pk_files");
        b.Property(x => x.Id).HasColumnName("id");

        b.Property(x => x.OrgId).HasColumnName("org_id").IsRequired();
        b.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
        b.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
        b.Property(x => x.ContentType).HasColumnName("content_type").IsRequired();
        b.Property(x => x.StorageKey).HasColumnName("storage_key").IsRequired();
        b.Property(x => x.StorageUrl).HasColumnName("storage_url");
        b.Property(x => x.SizeBytes).HasColumnName("size_bytes").IsRequired();
        b.Property(x => x.ChecksumSha256).HasColumnName("checksum_sha256");
        b.Property(x => x.Pages).HasColumnName("pages");
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(x => new { x.OrgId, x.OwnerUserId }).HasDatabaseName("ix_files_org_owner");
        b.HasIndex(x => x.ChecksumSha256).HasDatabaseName("ix_files_checksum");

        b.HasOne<OrgRecord>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_files_org")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasConstraintName("fk_files_owner")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
