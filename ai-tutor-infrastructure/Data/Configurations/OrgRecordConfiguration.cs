// <copyright file="OrgRecordConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class OrgRecordConfiguration : IEntityTypeConfiguration<OrgRecord>
{
    public void Configure(EntityTypeBuilder<OrgRecord> b)
    {
        b.ToTable("orgs");
        b.HasKey(x => x.Id).HasName("pk_orgs");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_orgs_slug");
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>();
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}