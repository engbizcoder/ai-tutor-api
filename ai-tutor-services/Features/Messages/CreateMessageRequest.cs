namespace Ai.Tutor.Services.Features.Messages;

using Domain.Entities;
using Domain.Enums;
using Mediation;

public sealed class CreateMessageRequest : IRequest<ChatMessage>
{
    public Guid OrgId { get; init; }

    public Guid ThreadId { get; init; }

    public string Content { get; init; } = string.Empty;

    public SenderType SenderType { get; init; }

    public Guid? SenderId { get; init; }

    public string? MetadataJson { get; init; }

    public string? IdempotencyKey { get; init; }
}
