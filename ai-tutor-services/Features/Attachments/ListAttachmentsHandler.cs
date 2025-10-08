namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed class ListAttachmentsHandler(
    IAttachmentRepository attachments,
    ILogger<ListAttachmentsHandler> logger) : IRequestHandler<ListAttachmentsRequest, IReadOnlyList<Attachment>>
{
    public async Task<IReadOnlyList<Attachment>> Handle(ListAttachmentsRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Listing attachments for message {MessageId}", request.MessageId);
        var items = await attachments.ListByMessageIdAsync(request.MessageId, ct);
        logger.LogInformation("Listed {Count} attachments", items.Count);
        return items;
    }
}
