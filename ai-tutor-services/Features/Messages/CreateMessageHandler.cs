namespace Ai.Tutor.Services.Features.Messages;

using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediation;
using Microsoft.Extensions.Logging;

public sealed class CreateMessageHandler(
    IMessageRepository messages,
    IThreadRepository threads,
    ILogger<CreateMessageHandler> logger) : IRequestHandler<CreateMessageRequest, ChatMessage>
{
    public async Task<ChatMessage> Handle(CreateMessageRequest request, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating message for thread {ThreadId} in org {OrgId}",
            request.ThreadId,
            request.OrgId);

        // Validate thread exists and belongs to org
        _ = await threads.GetAsync(request.ThreadId, request.OrgId, ct) ?? throw new ThreadNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}");

        // Check for existing message with the same idempotency key
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingMessage = await messages.GetByIdempotencyKeyAsync(request.IdempotencyKey, request.OrgId, ct);
            if (existingMessage != null)
            {
                logger.LogInformation("Found existing message {MessageId} for idempotency key", existingMessage.Id);
                return existingMessage;
            }
        }

        var entity = new ChatMessage
        {
            ThreadId = request.ThreadId,
            SenderType = request.SenderType,
            SenderId = request.SenderId,
            Status = MessageStatus.Sent,
            Content = request.Content.Trim(),
            MetadataJson = request.MetadataJson,
            IdempotencyKey = request.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var result = await messages.AddAsync(entity, ct);

        logger.LogInformation("Message {MessageId} created successfully", result.Id);

        return result;
    }
}
