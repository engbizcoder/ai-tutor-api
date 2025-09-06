namespace Ai.Tutor.Services.Features.Messages;

using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediation;
using Microsoft.Extensions.Logging;

public sealed class ListMessagesHandler(
    IMessageRepository messages,
    IThreadRepository threads,
    ILogger<ListMessagesHandler> logger) : IRequestHandler<ListMessagesRequest, (IReadOnlyList<ChatMessage> Items, string? NextCursor)>
{
    public async Task<(IReadOnlyList<ChatMessage> Items, string? NextCursor)> Handle(ListMessagesRequest request, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Listing messages for thread {ThreadId} in org {OrgId} with page size {PageSize}", 
            request.ThreadId,
            request.OrgId,
            request.PageSize);

        // Validate thread exists and belongs to org
        _ = await threads.GetAsync(request.ThreadId, request.OrgId, ct) ?? throw new ThreadNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}");
        var result = await messages.ListByThreadPagedAsync(request.ThreadId, request.PageSize, request.Cursor, ct);

        logger.LogInformation("Listed {MessageCount} messages successfully", result.Items.Count);

        return result;
    }
}
