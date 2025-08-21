// <copyright file="UserRecordConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserRecordConfiguration : IEntityTypeConfiguration<UserRecord>
{
    public void Configure(EntityTypeBuilder<UserRecord> b)
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
        b.HasOne<OrgRecord>()
            .WithMany()
            .HasForeignKey(x => x.PrimaryOrgId)
            .HasConstraintName("fk_users_primary_org")
            .OnDelete(DeleteBehavior.Restrict);
    }
}