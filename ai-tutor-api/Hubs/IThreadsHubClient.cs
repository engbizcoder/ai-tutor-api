namespace Ai.Tutor.Api.Hubs;

using System;
using System.Threading.Tasks;
using Contracts.DTOs;

public interface IThreadsHubClient
{
    Task MessageCreated(MessageDto message);

    Task MessageUpdated(MessageDto message);

    Task MessageDeleted(Guid messageId, Guid threadId);

    Task TypingIndicator(Guid threadId, Guid? userId, string? userName, bool isTyping);

    Task ErrorOccurred(SignalRErrorDto errorDto);
}
