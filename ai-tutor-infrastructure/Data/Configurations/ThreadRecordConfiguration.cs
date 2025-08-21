// <copyright file="ThreadRecordConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ThreadRecordConfiguration : IEntityTypeConfiguration<ThreadRecord>
{
    public void Configure(EntityTypeBuilder<ThreadRecord> builder)
    {
        builder.ToTable("chat_threads");
        builder.HasKey(x => x.Id).HasName("pk_chat_threads");
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.OrgId).HasColumnName("org_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.FolderId).HasColumnName("folder_id");

        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>(); // mapped to PG enum via DbContext
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").HasColumnType("numeric(18,6)").HasDefaultValue(1000m);
        builder.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.OrgId, x.FolderId, x.SortOrder, x.Id }).HasDatabaseName("ix_threads_org_folder_sort_id");
        builder.HasIndex(x => new { x.UserId, x.SortOrder, x.Id }).HasDatabaseName("ix_threads_user_sort_id");

        // Foreign keys
        builder.HasOne<OrgRecord>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_threads_org")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_threads_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FolderRecord>()
            .WithMany()
            .HasForeignKey(x => x.FolderId)
            .HasConstraintName("fk_threads_folder")
            .OnDelete(DeleteBehavior.Cascade);
    }
}