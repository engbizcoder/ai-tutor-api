namespace Ai.Tutor.Infrastructure.Data.Models;

using Domain.Enums;
using Interfaces;

public sealed class AttachmentRecord : ICreatedAtEntity
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public Guid FileId { get; set; }

    public AttachmentType Type { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public MessageRecord? Message { get; set; }

    public FileRecord? File { get; set; }
}
