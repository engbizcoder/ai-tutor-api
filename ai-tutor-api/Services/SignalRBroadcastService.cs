namespace Ai.Tutor.Api.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Ai.Tutor.Api.Hubs;
using Contracts.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public sealed class SignalRBroadcastService(
    IHubContext<ThreadsHub, IThreadsHubClient> hubContext,
    ILogger<SignalRBroadcastService> logger)
    : ISignalRBroadcastService
{
    public async Task BroadcastMessageCreatedAsync(ChatMessage message, CancellationToken ct = default)
    {
        var dto = Map(message);
        var group = GetThreadGroup(message.ThreadId);
        try
        {
            await hubContext.Clients.Group(group).MessageCreated(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast MessageCreated for ThreadId={ThreadId} MessageId={MessageId}", message.ThreadId, message.Id);

            // swallow to avoid impacting API request flow
        }
    }

    public async Task BroadcastMessageUpdatedAsync(ChatMessage message, CancellationToken ct = default)
    {
        var dto = Map(message);
        var group = GetThreadGroup(message.ThreadId);
        try
        {
            await hubContext.Clients.Group(group).MessageUpdated(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast MessageUpdated for ThreadId={ThreadId} MessageId={MessageId}", message.ThreadId, message.Id);
        }
    }

    public async Task BroadcastMessageDeletedAsync(Guid messageId, Guid threadId, CancellationToken ct = default)
    {
        var group = GetThreadGroup(threadId);
        try
        {
            await hubContext.Clients.Group(group).MessageDeleted(messageId, threadId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast MessageDeleted for ThreadId={ThreadId} MessageId={MessageId}", threadId, messageId);
        }
    }

    public async Task BroadcastTypingIndicatorAsync(Guid threadId, Guid? userId, string? userName, bool isTyping, CancellationToken ct = default)
    {
        var group = GetThreadGroup(threadId);
        try
        {
            await hubContext.Clients.Group(group).TypingIndicator(threadId, userId, userName, isTyping);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast TypingIndicator for ThreadId={ThreadId}, UserId={UserId}, IsTyping={IsTyping}", threadId, userId, isTyping);
        }
    }

    private static string GetThreadGroup(Guid threadId) => $"thread:{threadId}";

    private static MessageDto Map(ChatMessage x) => new()
    {
        Id = x.Id,
        ThreadId = x.ThreadId,
        SenderType = (Contracts.Enums.SenderType)x.SenderType,
        SenderId = x.SenderId,
        Status = (Contracts.Enums.MessageStatus)x.Status,
        Content = x.Content,
        MetadataJson = x.MetadataJson,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
