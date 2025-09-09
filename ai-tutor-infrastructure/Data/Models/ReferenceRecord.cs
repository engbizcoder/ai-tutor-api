namespace Ai.Tutor.Infrastructure.Data.Models;

using Domain.Enums;
using Interfaces;

public sealed class ReferenceRecord : ICreatedAtEntity
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public Guid? MessageId { get; set; }

    public ReferenceType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public Guid? FileId { get; set; }

    public int? PageNumber { get; set; }

    public string? PreviewImgUrl { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public ThreadRecord? Thread { get; set; }

    public MessageRecord? Message { get; set; }

    public FileRecord? File { get; set; }
}
