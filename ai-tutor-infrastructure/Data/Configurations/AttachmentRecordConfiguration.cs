namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;

public sealed class AttachmentRecordConfiguration : IEntityTypeConfiguration<AttachmentRecord>
{
    public void Configure(EntityTypeBuilder<AttachmentRecord> b)
    {
        b.ToTable("attachments");
        b.HasKey(x => x.Id).HasName("pk_attachments");
        b.Property(x => x.Id).HasColumnName("id");

        b.Property(x => x.MessageId).HasColumnName("message_id").IsRequired();
        b.Property(x => x.FileId).HasColumnName("file_id").IsRequired();
        b.Property(x => x.Type).HasColumnName("type");
        b.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        b.HasIndex(x => x.MessageId).HasDatabaseName("ix_attachments_message");

        b.HasOne<MessageRecord>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .HasConstraintName("fk_attachments_message")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<FileRecord>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .HasConstraintName("fk_attachments_file")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
