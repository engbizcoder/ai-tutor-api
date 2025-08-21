namespace Ai.Tutor.Infrastructure.Data.Models;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Infrastructure.Data.Interfaces;

public sealed class MessageRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public SenderType SenderType { get; set; } = SenderType.User;

    public Guid? SenderId { get; set; }

    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    public string Content { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ThreadRecord? Thread { get; set; }
}
