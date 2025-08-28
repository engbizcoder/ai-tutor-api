// <copyright file="OrgMemberRecordConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

public sealed class OrgMemberRecordConfiguration : IEntityTypeConfiguration<OrgMemberRecord>
{
    public void Configure(EntityTypeBuilder<OrgMemberRecord> b)
    {
        b.ToTable("org_members");
        b.HasKey(x => new { x.OrgId, x.UserId }).HasName("pk_org_members");
        b.Property(x => x.OrgId).HasColumnName("org_id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.Role).HasColumnName("role").HasConversion<string>();
        b.Property(x => x.JoinedAt).HasColumnName("joined_at").IsRequired();
        b.HasOne<OrgRecord>()
            .WithMany()
            .HasForeignKey(x => x.OrgId)
            .HasConstraintName("fk_org_members_orgs")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_org_members_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}