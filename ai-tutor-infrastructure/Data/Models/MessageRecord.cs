namespace Ai.Tutor.Infrastructure.Data.Models;

using Domain.Enums;
using Interfaces;

public sealed class MessageRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public SenderType SenderType { get; set; } = SenderType.User;

    public Guid? SenderId { get; set; }

    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    public string Content { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ThreadRecord? Thread { get; set; }
}
