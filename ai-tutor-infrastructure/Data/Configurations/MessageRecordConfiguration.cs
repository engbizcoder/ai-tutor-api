namespace Ai.Tutor.Infrastructure.Data.Configurations;

using Ai.Tutor.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class MessageRecordConfiguration : IEntityTypeConfiguration<MessageRecord>
{
    public void Configure(EntityTypeBuilder<MessageRecord> builder)
    {
        builder.ToTable("chat_messages");
        builder.HasKey(x => x.Id).HasName("pk_chat_messages");
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ThreadId).HasColumnName("thread_id").IsRequired();
        builder.Property(x => x.SenderType).HasColumnName("sender_type").HasConversion<string>();
        builder.Property(x => x.SenderId).HasColumnName("sender_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.ThreadId, x.CreatedAt, x.Id }).HasDatabaseName("ix_messages_thread_created_id");

        builder.HasOne(m => m.Thread)
            .WithMany(t => t.Messages)
            .HasForeignKey(x => x.ThreadId)
            .HasConstraintName("fk_messages_thread")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
