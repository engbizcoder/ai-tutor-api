namespace Ai.Tutor.Api.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

public interface ISignalRBroadcastService
{
    Task BroadcastMessageCreatedAsync(ChatMessage message, CancellationToken ct = default);

    Task BroadcastMessageUpdatedAsync(ChatMessage message, CancellationToken ct = default);

    Task BroadcastMessageDeletedAsync(Guid messageId, Guid threadId, CancellationToken ct = default);
}
