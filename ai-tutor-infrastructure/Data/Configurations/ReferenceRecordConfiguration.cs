namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

public sealed class ReferenceRecordConfiguration : IEntityTypeConfiguration<ReferenceRecord>
{
    public void Configure(EntityTypeBuilder<ReferenceRecord> builder)
    {
        builder.ToTable("document_references");
        builder.HasKey(x => x.Id).HasName("pk_document_references");
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ThreadId).HasColumnName("thread_id").IsRequired();
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Title).HasColumnName("title").IsRequired();
        builder.Property(x => x.Url).HasColumnName("url");
        builder.Property(x => x.FileId).HasColumnName("file_id");
        builder.Property(x => x.PageNumber).HasColumnName("page_number");
        builder.Property(x => x.PreviewImgUrl).HasColumnName("preview_img_url");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => new { x.ThreadId, x.CreatedAt, x.Id }).HasDatabaseName("ix_document_references_thread_created_id");
        builder.HasIndex(x => x.MessageId).HasDatabaseName("ix_document_references_message");
        builder.HasIndex(x => x.FileId).HasDatabaseName("ix_document_references_file_id");

        builder.HasOne<ThreadRecord>()
            .WithMany()
            .HasForeignKey(x => x.ThreadId)
            .HasConstraintName("fk_document_references_thread")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MessageRecord>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .HasConstraintName("fk_document_references_message")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FileRecord>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("fk_document_references_file")
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_document_references_url_or_file", "(url IS NOT NULL) OR (file_id IS NOT NULL)"));
    }
}
